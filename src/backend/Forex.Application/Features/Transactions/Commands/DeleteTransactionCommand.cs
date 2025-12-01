namespace Forex.Application.Features.Transactions.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed record DeleteTransactionCommand(long TransactionId) : IRequest<bool>;

public class DeleteTransactionCommandHandler(
    IAppDbContext context)
    : IRequestHandler<DeleteTransactionCommand, bool>
{
    public async Task<bool> Handle(DeleteTransactionCommand request, CancellationToken ct)
    {
        await context.BeginTransactionAsync(ct);

        try
        {
            var transaction = await LoadTransactionAsync(request.TransactionId, ct);

            await RevertUserAccountAsync(transaction, ct);
            RevertShopAccount(transaction);

            RemoveOperationRecord(transaction);
            RemoveTransaction(transaction);

            return await context.CommitTransactionAsync(ct);
        }
        catch
        {
            await context.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task<Transaction> LoadTransactionAsync(long transactionId, CancellationToken ct)
    {
        return await context.Transactions
            .Include(t => t.Shop)
                .ThenInclude(s => s.ShopAccounts)
            .Include(t => t.OperationRecord)
            .FirstOrDefaultAsync(t => t.Id == transactionId, ct)
            ?? throw new NotFoundException(nameof(Transaction), nameof(transactionId), transactionId);
    }

    private async Task RevertUserAccountAsync(Transaction transaction, CancellationToken ct)
    {
        var uzsCurrency = await context.Currencies
            .FirstOrDefaultAsync(c => c.Code == "UZS", ct)
            ?? throw new InvalidOperationException("UZS currency not found");

        var userAccount = await context.UserAccounts
            .FirstOrDefaultAsync(a => a.UserId == transaction.UserId && a.CurrencyId == uzsCurrency.Id, ct)
            ?? throw new NotFoundException(nameof(UserAccount), nameof(transaction.UserId), transaction.UserId);

        var amountInUZS = transaction.Amount * transaction.ExchangeRate;
        var delta = amountInUZS + transaction.Discount;

        userAccount.Balance -= delta;
    }

    private static void RevertShopAccount(Transaction transaction)
    {
        var shopAccount = transaction.Shop.ShopAccounts
            .FirstOrDefault(sa => sa.CurrencyId == transaction.CurrencyId)
            ?? throw new NotFoundException(nameof(ShopAccount), nameof(transaction.CurrencyId), transaction.CurrencyId);

        if (transaction.PaymentMethod == PaymentMethod.Naqd)
        {
            shopAccount.Balance -= transaction.Amount;
            if (shopAccount.Balance < 0)
                throw new ConflictException("Do'kon kassasida mablag' yetarli emas!");
        }
    }

    private void RemoveOperationRecord(Transaction transaction)
    {
        if (transaction.OperationRecord is not null)
        {
            context.OperationRecords.Remove(transaction.OperationRecord);
            transaction.OperationRecord = null!;
        }
    }

    private void RemoveTransaction(Transaction transaction)
    {
        context.Transactions.Remove(transaction);
    }
}
