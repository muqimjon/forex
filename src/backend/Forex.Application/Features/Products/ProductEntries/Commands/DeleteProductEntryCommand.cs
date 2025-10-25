namespace Forex.Application.Features.Products.ProductEntries.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteProductEntryCommand(long ProductEntryId) : IRequest<bool>;

public class DeleteProductEntryCommandHandler(IAppDbContext context)
    : IRequestHandler<DeleteProductEntryCommand, bool>
{
    public async Task<bool> Handle(DeleteProductEntryCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            var entry = await context.ProductEntries
                .FirstOrDefaultAsync(e => e.Id == request.ProductEntryId, cancellationToken);

            if (entry is null) return false;

            var productType = await context.ProductTypes
                .Include(pt => pt.ProductResidue)
                .Include(pt => pt.ProductTypeItems)
                .FirstOrDefaultAsync(pt => pt.Id == entry.ProductTypeId, cancellationToken)
                ?? throw new NotFoundException(nameof(ProductType), nameof(entry.ProductTypeId), entry.ProductTypeId);

            var employee = await context.Users
                .Include(u => u.Accounts)
                .FirstOrDefaultAsync(u => u.Id == entry.EmployeeId, cancellationToken)
                ?? throw new NotFoundException(nameof(User), nameof(entry.EmployeeId), entry.EmployeeId);

            await RevertProductResidueAsync(productType, entry.BundleCount, entry.ShopId, cancellationToken);
            await RevertSemiProductResiduesAsync(productType, entry.BundleCount * entry.BundleItemCount, cancellationToken);
            await RevertEmployeeBalanceAsync(employee, entry.TotalAmount);

            context.ProductEntries.Remove(entry);

            return await context.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task RevertProductResidueAsync(ProductType productType, int count, long shopId, CancellationToken ct)
    {
        var residue = await context.ProductResidues
            .FirstOrDefaultAsync(r => r.ProductTypeId == productType.Id && r.ShopId == shopId, ct);

        if (residue is not null)
            residue.Count -= count;
    }

    private async Task RevertSemiProductResiduesAsync(ProductType productType, int countByType, CancellationToken ct)
    {
        foreach (var item in productType.ProductTypeItems)
        {
            var semiResidue = await context.SemiProductResidues
                .FirstOrDefaultAsync(r => r.SemiProductId == item.SemiProductId, ct);

            if (semiResidue is not null)
                semiResidue.Quantity += item.Quantity * countByType;
        }
    }

    private static Task RevertEmployeeBalanceAsync(User employee, decimal amount)
    {
        var account = employee.Accounts
            .OfType<UserAccount>()
            .FirstOrDefault(a => a.CurrencyId == 1);

        if (account is not null)
            account.Balance -= amount;

        return Task.CompletedTask;
    }
}