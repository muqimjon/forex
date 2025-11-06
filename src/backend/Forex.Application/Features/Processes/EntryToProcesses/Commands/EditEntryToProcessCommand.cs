namespace Forex.Application.Features.Processes.EntryToProcesses.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Processes;
using Forex.Domain.Entities.Products;
using Forex.Domain.Entities.SemiProducts;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record EditEntryToProcessCommand(long Id, long ProductTypeId, decimal NewQuantity) : IRequest<bool>;

public class EditEntryToProcessCommandHandler(IAppDbContext context)
    : IRequestHandler<EditEntryToProcessCommand, bool>
{
    public async Task<bool> Handle(EditEntryToProcessCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            var entry = await GetEntryAsync(request.Id, cancellationToken);
            var oldProductType = await GetProductTypeAsync(entry.ProductTypeId, cancellationToken);
            await RevertEntryAsync(entry, oldProductType, cancellationToken);

            var newProductType = await GetProductTypeAsync(request.ProductTypeId, cancellationToken);
            var inProcess = await GetOrAttachInProcessAsync(request.ProductTypeId, cancellationToken);
            await ApplyNewEntryAsync(entry, newProductType, inProcess, request.NewQuantity, cancellationToken);

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
        var entry = await context.EntryToProcesses
            .Include(e => e.InProcess)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        return entry ?? throw new NotFoundException("EntryToProcess", "Id", id);
    }

    private async Task<ProductType> GetProductTypeAsync(long productTypeId, CancellationToken ct)
    {
        var productType = await context.ProductTypes
            .Include(pt => pt.ProductTypeItems)
            .FirstOrDefaultAsync(pt => pt.Id == productTypeId, ct);

        return productType ?? throw new NotFoundException("ProductType", "Id", productTypeId);
    }

    private async Task RevertEntryAsync(EntryToProcess entry, ProductType productType, CancellationToken ct)
    {
        foreach (var item in productType.ProductTypeItems)
        {
            var residue = await GetResidueAsync(item.SemiProductId, ct);
            residue.Quantity += item.Quantity * entry.Quantity;
        }

        var inProcess = await context.InProcesses.FirstOrDefaultAsync(p => p.ProductTypeId == entry.ProductTypeId, ct);
        if (inProcess is not null)
        {
            inProcess.Quantity -= entry.Quantity;
            if (inProcess.Quantity < 0)
                throw new ForbiddenException($"InProcess qoldiq manfiy bo‘lishi mumkin emas. ProductTypeId={entry.ProductTypeId}");
        }
    }

    private async Task ApplyNewEntryAsync(EntryToProcess entry, ProductType productType, InProcess inProcess, decimal newQuantity, CancellationToken ct)
    {
        foreach (var item in productType.ProductTypeItems)
        {
            var residue = await GetResidueAsync(item.SemiProductId, ct);
            var required = item.Quantity * newQuantity;

            if (residue.Quantity < required)
                throw new ForbiddenException($"Yetarli qoldiq yo‘q. SemiProductId={item.SemiProductId}, Available={residue.Quantity}, Required={required}");

            residue.Quantity -= required;
        }

        inProcess.Quantity += newQuantity;

        entry.ProductTypeId = productType.Id;
        entry.Quantity = newQuantity;
        entry.InProcess = inProcess;
    }

    private async Task<InProcess> GetOrAttachInProcessAsync(long productTypeId, CancellationToken ct)
    {
        var inProcess = await context.InProcesses.FirstOrDefaultAsync(p => p.ProductTypeId == productTypeId, ct);

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

    private async Task<SemiProductResidue> GetResidueAsync(long semiProductId, CancellationToken ct)
    {
        var residue = await context.SemiProductResidues.FirstOrDefaultAsync(r => r.SemiProductId == semiProductId, ct);
        return residue ?? throw new NotFoundException("SemiProductResidue", "SemiProductId", semiProductId);
    }
}
