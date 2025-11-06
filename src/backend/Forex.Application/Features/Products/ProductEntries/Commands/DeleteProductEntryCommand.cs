namespace Forex.Application.Features.Products.ProductEntries.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Processes;
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
            var totalCount = entry.BundleCount * entry.BundleItemCount;

            var residue = await GetProductResidueAsync(entry.ProductTypeId, entry.ShopId, cancellationToken);
            residue.Count -= entry.BundleCount;

            if (residue.Count < 0)
                throw new ForbiddenException($"Mahsulot qoldig'i manfiy bo'lishi mumkin emas. ProductTypeId={entry.ProductTypeId}");

            var inProcess = await GetOrCreateInProcessAsync(entry.ProductTypeId, cancellationToken);
            inProcess.Quantity += totalCount;

            context.ProductEntries.Remove(entry);

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

    private async Task<InProcess> GetOrCreateInProcessAsync(long productTypeId, CancellationToken ct)
    {
        var inProcess = await context.InProcesses
            .FirstOrDefaultAsync(p => p.ProductTypeId == productTypeId, ct);

        if (inProcess is null)
        {
            inProcess = new InProcess
            {
                ProductTypeId = productTypeId,
                Quantity = 0
            };

            await context.InProcesses.AddAsync(inProcess, ct);
        }

        return inProcess;
    }
}
