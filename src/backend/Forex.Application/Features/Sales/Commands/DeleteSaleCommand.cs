namespace Forex.Application.Features.Sales.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;
using Forex.Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteSaleCommand(long SaleId) : IRequest<bool>;

public class DeleteSaleCommandHandler(
    IAppDbContext context)
    : IRequestHandler<DeleteSaleCommand, bool>
{
    public async Task<bool> Handle(DeleteSaleCommand request, CancellationToken ct)
    {
        await context.BeginTransactionAsync(ct);

        try
        {
            var sale = await LoadSaleAsync(request.SaleId, ct);
            var userAccount = sale.User.Accounts.FirstOrDefault()
                ?? throw new NotFoundException(nameof(UserAccount), nameof(sale.CustomerId), sale.CustomerId);

            userAccount.Balance += sale.TotalAmount;

            var productResidues = await LoadProductResiduesAsync(sale.SaleItems, ct);

            RevertProductResidueCounts(sale.SaleItems, productResidues);

            context.SaleItems.RemoveRange(sale.SaleItems);
            context.Sales.Remove(sale);

            await context.SaveAsync(ct);
            return await context.CommitTransactionAsync(ct);
        }
        catch
        {
            await context.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task<Sale> LoadSaleAsync(long saleId, CancellationToken ct)
    {
        var sale = await context.Sales
            .Include(s => s.SaleItems)
            .Include(s => s.User)
                .ThenInclude(u => u.Accounts)
            .FirstOrDefaultAsync(s => s.Id == saleId, ct);

        return sale ?? throw new NotFoundException(nameof(Sale), nameof(saleId), saleId);
    }

    private Task<List<ProductResidue>> LoadProductResiduesAsync(IEnumerable<SaleItem> saleItems, CancellationToken ct) =>
        context.ProductResidues
            .Where(r => saleItems.Select(si => si.ProductTypeId).Distinct().Contains(r.ProductTypeId))
            .ToListAsync(ct);

    private void RevertProductResidueCounts(IEnumerable<SaleItem> saleItems, List<ProductResidue> residues)
    {
        foreach (var item in saleItems)
        {
            var residue = residues.FirstOrDefault(r => r.ProductTypeId == item.ProductTypeId)
                ?? throw new NotFoundException(nameof(ProductResidue), nameof(item.ProductTypeId), item.ProductTypeId);

            residue.Count += item.BundleCount;
        }
    }
}
