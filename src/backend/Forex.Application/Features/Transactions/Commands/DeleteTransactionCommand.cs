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
    public async Task<bool> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            var transaction = await context.Transactions
                .Include(t => t.Shop)
                    .ThenInclude(s => s.ShopAccounts)
                .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken)
                ?? throw new NotFoundException("Transaction not found");

            await RevertUserAccountAsync(transaction, cancellationToken);
            RevertShopAccount(transaction);

            context.Transactions.Remove(transaction);

            return await context.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task RevertUserAccountAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        var uzsCurrency = await context.Currencies
            .FirstOrDefaultAsync(c => c.Code == "UZS", cancellationToken)
            ?? throw new InvalidOperationException("UZS currency not found");

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
            if (shopAccount.Balance < 0)
                throw new ConflictException("Do'kon kassasida mablag' yetarli emas!");
        }
    }
}
