namespace Forex.Application.Features.Products.ProductEntries.Commands;

using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.Products.Products.Commands;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateProductEntryCommand(
    long Id,
    DateTime Date,
    int Count,
    int BundleItemCount,
    decimal PreparationCostPerUnit,
    decimal UnitPrice,
    ProductionOrigin ProductionOrigin,
    ProductCommand Product)
    : IRequest<long>;

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
                ?? throw new NotFoundException("ProductEntry topilmadi!");
            
            // 1. Delete existing entry
            await mediator.Send(new DeleteProductEntryCommand(request.Id), cancellationToken);

            // 2. Map request to CreateProductEntryCommand
            var createCommand = new CreateProductEntryCommand(new ProductEntryCommand
            {
                Id = 0,
                Date = request.Date,
                Count = request.Count,
                BundleItemCount = request.BundleItemCount,
                PreparationCostPerUnit = request.PreparationCostPerUnit,
                UnitPrice = request.UnitPrice,
                ProductionOrigin = request.ProductionOrigin,
                Product = request.Product
            });

            var newId = await mediator.Send(createCommand, cancellationToken);
            
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