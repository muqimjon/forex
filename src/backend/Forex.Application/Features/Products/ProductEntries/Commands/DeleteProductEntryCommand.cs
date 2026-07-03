namespace Forex.Application.Features.Products.ProductEntries.Commands;

using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Interfaces;
using Forex.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteProductEntryCommand(long Id) : IRequest<bool>;

public class DeleteProductEntryCommandHandler(IAppDbContext context)
    : IRequestHandler<DeleteProductEntryCommand, bool>
{
    public async Task<bool> Handle(DeleteProductEntryCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            var entry = await GetEntryAsync(request.Id, cancellationToken);
            var residue = await GetProductResidueAsync(entry.ProductTypeId, entry.ShopId, cancellationToken);
            residue.Count -= entry.Count;

            if (residue.Count < 0)
                throw new ForbiddenException($"Mahsulot qoldig'i manfiy bo'lishi mumkin emas. ProductTypeId={entry.ProductTypeId}");

            context.ProductEntries.Remove(entry);

            // Drop the residue once its last intake is gone, so no empty (0-count, 0-entry) residue lingers.
            var remainingEntries = await context.ProductEntries
                .CountAsync(e => e.ProductResidueId == residue.Id && e.Id != entry.Id, cancellationToken);
            if (remainingEntries == 0)
                context.ProductResidues.Remove(residue);

            return await context.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<ProductEntry> GetEntryAsync(long id, CancellationToken ct)
    {
        var entry = await context.ProductEntries
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        return entry is null ? throw new NotFoundException("ProductEntry", "Id", id) : entry;
    }

    private async Task<ProductResidue> GetProductResidueAsync(long productTypeId, long shopId, CancellationToken ct)
    {
        var residue = await context.ProductResidues
            .FirstOrDefaultAsync(r => r.ProductTypeId == productTypeId && r.ShopId == shopId, ct);

        return residue is null ? throw new NotFoundException("ProductResidue", "ProductTypeId", productTypeId) : residue;
    }
}
