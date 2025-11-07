namespace Forex.Application.Features.Processes.EntryToProcesses.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Processes;
using Forex.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteEntryToProcessCommand(long EntryToProcessId) : IRequest<bool>;

public class DeleteEntryToProcessCommandHandler(IAppDbContext context)
    : IRequestHandler<DeleteEntryToProcessCommand, bool>
{
    public async Task<bool> Handle(DeleteEntryToProcessCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            var entry = await GetEntryAsync(request.EntryToProcessId, cancellationToken);
            var productType = await GetProductTypeAsync(entry.ProductTypeId, cancellationToken);

            await RestoreSemiProductResiduesAsync(entry, productType, cancellationToken);
            await RevertInProcessAsync(entry.ProductTypeId, entry.Count, cancellationToken);

            context.EntryToProcesses.Remove(entry);

            return await context.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<EntryToProcess> GetEntryAsync(long id, CancellationToken ct)
    {
        var entry = await context.EntryToProcesses.FirstOrDefaultAsync(e => e.Id == id, ct);
        return entry ?? throw new NotFoundException("EntryToProcess", "Id", id);
    }

    private async Task<ProductType> GetProductTypeAsync(long productTypeId, CancellationToken ct)
    {
        var productType = await context.ProductTypes
            .Include(pt => pt.ProductTypeItems)
            .FirstOrDefaultAsync(pt => pt.Id == productTypeId, ct);

        return productType ?? throw new NotFoundException("ProductType", "Id", productTypeId);
    }

    private async Task RestoreSemiProductResiduesAsync(EntryToProcess entry, ProductType productType, CancellationToken ct)
    {
        foreach (var item in productType.ProductTypeItems)
        {
            var residue = await context.SemiProductResidues
                .FirstOrDefaultAsync(r => r.SemiProductId == item.SemiProductId, ct);

            if (residue is null)
                throw new NotFoundException("SemiProductResidue", "SemiProductId", item.SemiProductId);

            residue.Quantity += item.Quantity * entry.Count;
        }
    }

    private async Task RevertInProcessAsync(long productTypeId, int count, CancellationToken ct)
    {
        var inProcess = await context.InProcesses.FirstOrDefaultAsync(p => p.ProductTypeId == productTypeId, ct);

        if (inProcess is null)
            return;

        inProcess.Count -= count;

        if (inProcess.Count < 0)
            throw new ForbiddenException($"InProcess qoldiq manfiy bo‘lishi mumkin emas. ProductTypeId={productTypeId}");
    }
}
