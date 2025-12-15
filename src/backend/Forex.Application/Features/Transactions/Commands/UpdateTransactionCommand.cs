namespace Forex.Application.Features.Transactions.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed record UpdateTransactionCommand(
    long Id,
    decimal Amount,
    decimal ExchangeRate,
    decimal Discount,
    PaymentMethod PaymentMethod,
    bool IsIncome,
    string? Description,
    DateTime Date,
    DateTime DueDate,
    long UserId,
    long CurrencyId)
    : IRequest<bool>;

public class UpdateTransactionCommandHandler(
    IAppDbContext context) : IRequestHandler<UpdateTransactionCommand, bool>
{
    public async Task<bool> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1) Mavjud tranzaksiyani olish
            var existingTransaction = await context.Transactions
                .Include(t => t.Shop)
                    .ThenInclude(s => s.ShopAccounts)
                .Include(t => t.OperationRecord) // OperationRecord ham yuklanadi
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(Transaction), nameof(request.Id), request.Id);

            // 2) Eski ma'lumotlarni revert qilish
            await RevertUserAccountAsync(existingTransaction, cancellationToken);
            RevertShopAccount(existingTransaction);

            // 3) Yangi ma'lumotlarni qo'llash
            await UpdateTransactionAsync(existingTransaction, request, cancellationToken);
            await UpdateUserAccountAsync(request, existingTransaction, cancellationToken);
            UpdateShopAccount(existingTransaction, request);
            await UpdateCurrencyExchangeRate(request.CurrencyId, request.ExchangeRate);

            // 4) Commit
            await context.CommitTransactionAsync(cancellationToken);
            return true;
        }
        catch
        {
            await context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task UpdateTransactionAsync(
        Transaction existingTransaction,
        UpdateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        // Transaction ma'lumotlarini yangilash
        existingTransaction.Amount = request.Amount;
        existingTransaction.ExchangeRate = request.ExchangeRate;
        existingTransaction.Discount = request.Discount;
        existingTransaction.PaymentMethod = request.PaymentMethod;
        existingTransaction.IsIncome = request.IsIncome;
        existingTransaction.Description = request.Description;
        existingTransaction.Date = request.Date.ToUtcSafe();
        existingTransaction.UserId = request.UserId;
        existingTransaction.CurrencyId = request.CurrencyId;

        // OperationRecord'ni yangilash
        var description = await GenerateDescription(existingTransaction);
        var amount = existingTransaction.Amount * existingTransaction.ExchangeRate +
                    (existingTransaction.IsIncome ? existingTransaction.Discount : 0);

        if (existingTransaction.OperationRecord is not null)
        {
            // mavjudini yangilash
            existingTransaction.OperationRecord.Amount = amount;
            existingTransaction.OperationRecord.Date = existingTransaction.Date.ToUtcSafe();
            existingTransaction.OperationRecord.Description = description;
            existingTransaction.OperationRecord.Type = OperationType.Transaction;
        }
        else
        {
            // yangisini yaratish
            existingTransaction.OperationRecord = new OperationRecord
            {
                Amount = amount,
                Date = existingTransaction.Date.ToUtcSafe(),
                Description = description,
                Type = OperationType.Transaction
            };
        }
    }

    private async Task<string> GenerateDescription(Transaction transaction)
    {
        var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Id == transaction.CurrencyId)
            ?? throw new NotFoundException(nameof(Currency), nameof(transaction.CurrencyId), transaction.CurrencyId);

        if (transaction.Discount < 0)
            throw new ForbiddenException("Chegirma 0 dan kichik bo'lishi mumkin emas!");

        var isWithDiscount = transaction.Discount > 0;

        return transaction.PaymentMethod switch
        {
            PaymentMethod.Naqd => $"Naqd to'lov: {transaction.Amount} {currency.Code}, Kurs: {transaction.ExchangeRate} UZS{(isWithDiscount ? $", Chegirma: {transaction.Discount} UZS" : string.Empty)}",
            PaymentMethod.Plastik => $"Karta to'lov: {transaction.Amount} {currency.Code}, Kurs: {transaction.ExchangeRate} UZS{(isWithDiscount ? $", Chegirma: {transaction.Discount} UZS" : string.Empty)}",
            PaymentMethod.HisobRaqam => $"Hisob raqam orqali to'lov: {transaction.Amount} {currency.Code}, Kurs: {transaction.ExchangeRate} UZS{(isWithDiscount ? $", Chegirma: {transaction.Discount} UZS" : string.Empty)}",
            PaymentMethod.MobilIlova => $"Online to'lov: {transaction.Amount} {currency.Code}, Kurs: {transaction.ExchangeRate} UZS{(isWithDiscount ? $", Chegirma: {transaction.Discount} UZS" : string.Empty)}",
            _ => "Noma'lum to'lov usuli",
        };
    }

    private async Task UpdateCurrencyExchangeRate(long currencyId, decimal exchangeRate)
    {
        var currency = await context.Currencies.FirstOrDefaultAsync(c => c.Id == currencyId)
            ?? throw new NotFoundException(nameof(Currency), nameof(currencyId), currencyId);

        currency.ExchangeRate = exchangeRate;
    }

    #region Revert Operations

    private async Task RevertUserAccountAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        var uzsCurrency = await context.Currencies
            .FirstOrDefaultAsync(c => c.Code == "UZS", cancellationToken)
            ?? throw new NotFoundException(nameof(Currency), nameof(Currency.Code), "UZS");

        var userAccount = await context.UserAccounts
            .FirstOrDefaultAsync(a => a.UserId == transaction.UserId && a.CurrencyId == uzsCurrency.Id, cancellationToken)
            ?? throw new NotFoundException("Customer account not found");

        var amountInUZS = transaction.Amount * transaction.ExchangeRate;
        var delta = amountInUZS + transaction.Discount;

        userAccount.Balance -= delta;
    }

    private static void RevertShopAccount(Transaction transaction)
    {
        var shopAccount = transaction.Shop.ShopAccounts
            .FirstOrDefault(sa => sa.CurrencyId == transaction.CurrencyId)
            ?? throw new NotFoundException("Shop account not found");

        if (transaction.PaymentMethod == PaymentMethod.Naqd)
        {
            shopAccount.Balance -= transaction.Amount;
        }
    }

    #endregion

    #region Apply New Operations

    private async Task UpdateUserAccountAsync(
        UpdateTransactionCommand request,
        Transaction transaction,
        CancellationToken cancellationToken)
    {
        var uzsCurrency = await context.Currencies
            .FirstOrDefaultAsync(c => c.Code == "UZS", cancellationToken)
            ?? throw new NotFoundException(nameof(Currency), nameof(Currency.Code), "UZS");

        var userAccount = await context.UserAccounts
            .FirstOrDefaultAsync(a => a.UserId == request.UserId && a.CurrencyId == uzsCurrency.Id, cancellationToken);

        if (userAccount is null)
        {
            userAccount = new UserAccount
            {
                UserId = request.UserId,
                CurrencyId = uzsCurrency.Id,
                OpeningBalance = 0,
                Balance = 0,
                Discount = 0
            };
            context.UserAccounts.Add(userAccount);
        }

        var amountInUZS = request.Amount * request.ExchangeRate;
        var delta = amountInUZS + request.Discount;

        userAccount.DueDate = DateTime.SpecifyKind(request.DueDate, DateTimeKind.Utc);
        userAccount.Balance += delta;
    }

    private static void UpdateShopAccount(Transaction transaction, UpdateTransactionCommand request)
    {
        var shopAccount = transaction.Shop.ShopAccounts
            .FirstOrDefault(sh => sh.CurrencyId == request.CurrencyId);

        if (shopAccount is null)
        {
            transaction.Shop.ShopAccounts.Add(shopAccount = new ShopAccount
            {
                CurrencyId = request.CurrencyId,
                OpeningBalance = 0,
                Balance = 0,
                Discount = 0
            });
        }

        if (request.PaymentMethod == PaymentMethod.Naqd)
        {
            shopAccount.Balance += request.Amount;
            if (shopAccount.Balance < 0)
                throw new ConflictException("Do'kon kassasida mablag' yetarli emas!");
        }
    }

    #endregion
}
