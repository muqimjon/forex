namespace Forex.Application.Features.Products.ProductEntries.Commands;

using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.Products.Products.Commands;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class UpdateProductEntryCommand : IRequest<long>
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public int BundleItemCount { get; set; }
    public int PackItemCount { get; set; }
    public decimal PreparationCostPerUnit { get; set; }
    public decimal UnitPrice { get; set; }
    public ProductionOrigin ProductionOrigin { get; set; }
    public ProductCommand Product { get; set; } = default!;
}

public class UpdateProductEntryCommandHandler(
    IMediator mediator,
    IAppDbContext context)
    : IRequestHandler<UpdateProductEntryCommand, long>
{
    public async Task<long> Handle(UpdateProductEntryCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            var existingEntry = await context.ProductEntries
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException("Mahsulot kirimi", nameof(request.Id), request.Id);

            await mediator.Send(new DeleteProductEntryCommand(request.Id), cancellationToken);

            var newId = await mediator.Send(new CreateProductEntryCommand
            {
                Date = request.Date,
                Count = request.Count,
                BundleItemCount = request.BundleItemCount,
                PackItemCount = request.PackItemCount,
                PreparationCostPerUnit = request.PreparationCostPerUnit,
                UnitPrice = request.UnitPrice,
                ProductionOrigin = request.ProductionOrigin,
                Product = request.Product
            }, cancellationToken);

            await context.CommitTransactionAsync(cancellationToken);
            return newId;
        }
        catch
        {
            await context.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}