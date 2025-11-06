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
            await ApplyNewEntryAsync(entry, request.ProductTypeId, request.NewQuantity, newProductType, cancellationToken);

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
        return entry is null ? throw new NotFoundException($"EntryToProcess not found. Id={id}") : entry;
    }

    private async Task<ProductType> GetProductTypeAsync(long productTypeId, CancellationToken ct)
    {
        var productType = await context.ProductTypes
            .Include(pt => pt.ProductTypeItems)
            .FirstOrDefaultAsync(pt => pt.Id == productTypeId, ct);

        if (productType is null)
            throw new NotFoundException($"ProductType not found. Id={productTypeId}");

        return productType;
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
                throw new InvalidOperationException($"InProcess quantity cannot be negative. ProductTypeId={entry.ProductTypeId}");
        }
    }

    private async Task ApplyNewEntryAsync(EntryToProcess entry, long newProductTypeId, decimal newQuantity, ProductType productType, CancellationToken ct)
    {
        foreach (var item in productType.ProductTypeItems)
        {
            var residue = await GetResidueAsync(item.SemiProductId, ct);
            var required = item.Quantity * newQuantity;

            if (residue.Quantity < required)
                throw new ForbiddenException($"Not enough residue. SemiProductId={item.SemiProductId}, Available={residue.Quantity}, Required={required}");

            residue.Quantity -= required;
        }

        var inProcess = await context.InProcesses.FirstOrDefaultAsync(p => p.ProductTypeId == newProductTypeId, ct);
        if (inProcess is null)
        {
            inProcess = new InProcess { ProductTypeId = newProductTypeId, Quantity = 0 };
            await context.InProcesses.AddAsync(inProcess, ct);
        }

        inProcess.Quantity += newQuantity;

        entry.ProductTypeId = newProductTypeId;
        entry.Quantity = newQuantity;
    }

    private async Task<SemiProductResidue> GetResidueAsync(long semiProductId, CancellationToken ct)
    {
        var residue = await context.SemiProductResidues.FirstOrDefaultAsync(r => r.SemiProductId == semiProductId, ct);
        return residue is null ? throw new NotFoundException($"SemiProductResidue not found. SemiProductId={semiProductId}") : residue;
    }
}
