namespace Forex.Application.Features.Processes.EntryToProcesses.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Processes;
using Forex.Domain.Entities.Products;
using Forex.Domain.Entities.SemiProducts;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record EntryToProcessCommand(long ProductTypeId, decimal Quantity) : IRequest<long>;

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

                foreach (var item in productType.ProductTypeItems)
                {
                    var requiredQuantity = item.Quantity * cmd.Quantity;
                    var semiResidue = await GetSemiProductResidueAsync(item.SemiProductId, cancellationToken);
                    ValidateResidueQuantity(semiResidue, requiredQuantity);

                    semiResidue.Quantity -= requiredQuantity;
                }

                var inProcess = await GetOrCreateInProcessAsync(cmd.ProductTypeId, cancellationToken);
                inProcess.Quantity += cmd.Quantity;

                await AddEntryToProcessAsync(cmd, cancellationToken);
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


    private async Task<ProductType> GetProductTypeAsync(long id, CancellationToken ct)
    {
        var productType = await context.ProductTypes
            .Include(pt => pt.ProductTypeItems)
            .ThenInclude(pti => pti.SemiProduct)
            .FirstOrDefaultAsync(pt => pt.Id == id, ct)
            ?? throw new NotFoundException($"ProductType not found. Id={id}");
        return productType;
    }

    private async Task<SemiProductResidue> GetSemiProductResidueAsync(long semiProductId, CancellationToken ct)
    {
        var residue = await context.SemiProductResidues
            .FirstOrDefaultAsync(r => r.SemiProductId == semiProductId, ct);

        if (residue is null)
            throw new NotFoundException($"SemiProductResidue not found. SemiProductId={semiProductId}");

        return residue;
    }

    private static void ValidateResidueQuantity(SemiProductResidue residue, decimal requiredQuantity)
    {
        if (residue.Quantity < requiredQuantity)
            throw new ForbiddenException(
                $"Not enough residue for SemiProductId={residue.SemiProductId}. Available={residue.Quantity}, Required={requiredQuantity}");
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

    private async Task AddEntryToProcessAsync(EntryToProcessCommand cmd, CancellationToken ct)
    {
        var entry = new EntryToProcess
        {
            ProductTypeId = cmd.ProductTypeId,
            Quantity = cmd.Quantity
        };

        await context.EntryToProcesses.AddAsync(entry, ct);
    }
}
