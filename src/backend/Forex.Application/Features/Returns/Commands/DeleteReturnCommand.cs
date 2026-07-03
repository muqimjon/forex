namespace Forex.Application.Features.Returns.Commands;

using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Domain.Entities.Products;
using Forex.Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteReturnCommand(long ReturnId) : IRequest<bool>;

public class DeleteReturnCommandHandler(
    IAppDbContext context)
    : IRequestHandler<DeleteReturnCommand, bool>
{
    public async Task<bool> Handle(DeleteReturnCommand request, CancellationToken ct)
    {
        await context.BeginTransactionAsync(ct);

        try
        {
            var @return = await LoadReturnAsync(request.ReturnId, ct);

            if (@return.OperationRecord is not null)
            {
                var account = await context.GetSettlementAccountAsync(@return.CustomerId, ct);
                account.Balance -= @return.OperationRecord.Amount * @return.OperationRecord.Rate;
            }

            var productResidues = await LoadProductResiduesAsync(@return.ReturnItems, ct);

            RevertProductResidueCounts(@return.ReturnItems, productResidues);

            context.ReturnItems.RemoveRange(@return.ReturnItems);

            if (@return.OperationRecord is not null)
            {
                context.OperationRecords.Remove(@return.OperationRecord);
                @return.OperationRecord = null!;
            }

            context.Returns.Remove(@return);

            return await context.CommitTransactionAsync(ct);
        }
        catch
        {
            await context.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task<Return> LoadReturnAsync(long returnId, CancellationToken ct)
    {
        var @return = await context.Returns
            .Include(r => r.ReturnItems)
            .Include(r => r.Customer)
                .ThenInclude(u => u.Accounts)
            .Include(r => r.OperationRecord)
            .FirstOrDefaultAsync(r => r.Id == returnId, ct);

        return @return ?? throw new NotFoundException(nameof(Return), nameof(returnId), returnId);
    }

    private Task<List<ProductResidue>> LoadProductResiduesAsync(IEnumerable<ReturnItem> returnItems, CancellationToken ct) =>
        context.ProductResidues
            .Where(r => returnItems.Select(ri => ri.ProductTypeId).Distinct().Contains(r.ProductTypeId))
            .ToListAsync(ct);

    private void RevertProductResidueCounts(IEnumerable<ReturnItem> returnItems, List<ProductResidue> residues)
    {
        foreach (var item in returnItems)
        {
            if (item.RestockCount <= 0) continue;

            var residue = residues.FirstOrDefault(r => r.ProductTypeId == item.ProductTypeId)
                ?? throw new NotFoundException(nameof(ProductResidue), nameof(item.ProductTypeId), item.ProductTypeId);

            residue.Count -= item.RestockCount;
        }
    }
}
