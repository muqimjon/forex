namespace Forex.Application.Features.Processes.EntryToProcesses.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteEntryToProcessCommand(long EntryToProcessId) : IRequest<bool>;

public class RevertEntryToProcessCommandHandler(IAppDbContext context)
    : IRequestHandler<DeleteEntryToProcessCommand, bool>
{
    public async Task<bool> Handle(DeleteEntryToProcessCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            var entry = await context.EntryToProcesses
                .FirstOrDefaultAsync(e => e.Id == request.EntryToProcessId, cancellationToken)
                ?? throw new NotFoundException($"EntryToProcess not found. Id={request.EntryToProcessId}");

            var productType = await context.ProductTypes
                .Include(pt => pt.ProductTypeItems)
                .FirstOrDefaultAsync(pt => pt.Id == entry.ProductTypeId, cancellationToken)
                ?? throw new NotFoundException($"ProductType not found. Id={entry.ProductTypeId}");

            foreach (var item in productType.ProductTypeItems)
            {
                var residue = await context.SemiProductResidues
                    .FirstOrDefaultAsync(r => r.SemiProductId == item.SemiProductId, cancellationToken)
                    ?? throw new NotFoundException($"SemiProductResidue not found. SemiProductId={item.SemiProductId}");

                var restoreQuantity = item.Quantity * entry.Quantity;
                residue.Quantity += restoreQuantity;
            }

            var inProcess = await context.InProcesses
                .FirstOrDefaultAsync(p => p.ProductTypeId == entry.ProductTypeId, cancellationToken);

            if (inProcess is not null)
            {
                inProcess.Quantity -= entry.Quantity;

                if (inProcess.Quantity < 0)
                    throw new InvalidOperationException($"InProcess quantity cannot be negative. ProductTypeId={entry.ProductTypeId}");
            }

            context.EntryToProcesses.Remove(entry);

            return await context.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
