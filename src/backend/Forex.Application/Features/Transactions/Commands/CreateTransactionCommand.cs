namespace Forex.Application.Features.Transactions.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed record CreateTransactionCommand(
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
    : IRequest<long>;

public class CreateTransactionCommandHandler(
    IAppDbContext context,
    IMapper mapper) : IRequestHandler<CreateTransactionCommand, long>
{
    public async Task<long> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            var transaction = await CreateTransactionAsync(request, cancellationToken);
            await UpdateUserAccountAsync(request, transaction, cancellationToken);
            UpdateShopAccountAsync(transaction);

            await context.CommitTransactionAsync(cancellationToken);
            return transaction.Id;
        }
        catch
        {
            await context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<Transaction> CreateTransactionAsync(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var shop = await context.Shops
            .Include(sh => sh.ShopAccounts)
            .FirstOrDefaultAsync(cancellationToken);

        if (shop is null)
        {
            shop = new Shop
            {
                Name = "Default",
                NormalizedName = "DEFAULT"
            };

            var shopAccount = new ShopAccount
            {
                CurrencyId = request.CurrencyId,
                OpeningBalance = 0,
                Balance = 0,
                Discount = 0,
                Shop = shop
            };

            context.ShopCashAccounts.Add(shopAccount);
            context.Shops.Add(shop);
        }

        var transaction = mapper.Map<Transaction>(request);
        transaction.Shop = shop;

        context.Transactions.Add(transaction);
        return transaction;
    }

    private async Task UpdateUserAccountAsync(CreateTransactionCommand request, Transaction transaction, CancellationToken cancellationToken)
    {
        var uzsCurrency = await context.Currencies
            .FirstOrDefaultAsync(c => c.Code == "UZS", cancellationToken)
            ?? throw new InvalidOperationException("UZS currency not found");

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

        userAccount.DueDate = request.DueDate.ToUtcSafe();
        userAccount.Balance += delta;
    }

    private static void UpdateShopAccountAsync(Transaction transaction)
    {
        var shopAccount = transaction.Shop.ShopAccounts.FirstOrDefault(sh => sh.CurrencyId == transaction.CurrencyId);

        if (shopAccount is null)
        {
            transaction.Shop.ShopAccounts.Add(shopAccount = new ShopAccount
            {
                CurrencyId = transaction.CurrencyId,
                OpeningBalance = 0,
                Balance = 0,
                Discount = 0
            });
        }

        if (transaction.PaymentMethod == PaymentMethod.Naqd)
        {
            shopAccount.Balance += transaction.Amount;
            if (shopAccount.Balance < 0)
                throw new ConflictException("Do'kon kassasida mablag' yetarli emas!");
        }
    }
}
