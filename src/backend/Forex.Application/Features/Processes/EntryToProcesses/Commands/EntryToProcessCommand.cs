namespace Forex.Application.Features.Processes.EntryToProcesses.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Processes;
using Forex.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record EntryToProcessCommand(long ProductTypeId, int Count) : IRequest<long>;
public record CreateEntryToProcessCommand(List<EntryToProcessCommand> Commands) : IRequest<long>;

public class CreateEntryToProcessCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateEntryToProcessCommand, long>
{
    public async Task<long> Handle(CreateEntryToProcessCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var cmd in request.Commands)
            {
                var productType = await GetProductTypeAsync(cmd.ProductTypeId, cancellationToken);
                await DeductSemiProductResiduesAsync(productType, cmd.Count, cancellationToken);
                var inProcess = await GetOrAttachInProcessAsync(cmd.ProductTypeId, cmd.Count, cancellationToken);
                await AddEntryToProcessAsync(cmd, inProcess, cancellationToken);
            }

            await context.CommitTransactionAsync(cancellationToken);
            return 1;
        }
        catch
        {
            await context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<ProductType> GetProductTypeAsync(long productTypeId, CancellationToken ct)
    {
        var productType = await context.ProductTypes
            .Include(pt => pt.ProductTypeItems)
            .FirstOrDefaultAsync(pt => pt.Id == productTypeId, ct);

        return productType is null ? throw new NotFoundException(nameof(ProductType), nameof(productTypeId), productTypeId) : productType;
    }

    private async Task DeductSemiProductResiduesAsync(ProductType productType, decimal quantity, CancellationToken ct)
    {
        foreach (var item in productType.ProductTypeItems)
        {
            var required = item.Quantity * quantity;

            var residue = await context.SemiProductResidues
                .FirstOrDefaultAsync(r => r.SemiProductId == item.SemiProductId, ct)
                ?? throw new NotFoundException("SemiProductResidue", "SemiProductId", item.SemiProductId);

            if (residue.Quantity < required)
                throw new ForbiddenException($"Yetarli qoldiq yo‘q. SemiProductId={item.SemiProductId}, Available={residue.Quantity}, Required={required}");

            residue.Quantity -= required;
        }
    }

    private async Task<InProcess> GetOrAttachInProcessAsync(long productTypeId, int count, CancellationToken ct)
    {
        var inProcess = await context.InProcesses
            .FirstOrDefaultAsync(p => p.ProductTypeId == productTypeId, ct);

        if (inProcess is null)
        {
            inProcess = new InProcess
            {
                ProductTypeId = productTypeId,
                Count = count
            };

            await context.InProcesses.AddAsync(inProcess, ct);
        }
        else
        {
            inProcess.Count += count;
        }

        return inProcess;
    }

    private async Task AddEntryToProcessAsync(EntryToProcessCommand cmd, InProcess inProcess, CancellationToken ct)
    {
        var entry = new EntryToProcess
        {
            ProductTypeId = cmd.ProductTypeId,
            Count = cmd.Count,
            InProcess = inProcess
        };

        await context.EntryToProcesses.AddAsync(entry, ct);
    }
}
