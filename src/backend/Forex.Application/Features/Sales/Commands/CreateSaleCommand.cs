namespace Forex.Application.Features.Sales.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Sales.DTOs;
using Forex.Domain.Entities.Sales;
using Forex.Domain.Entities.Shops;
using Forex.Domain.Entities.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record CreateSaleCommand(
    DateTime Date,
    long UserId,
    int TotalCount,
    decimal TotalAmount,
    string? Note,
    List<SaleItemCreateDto> SaleItems

    ) : IRequest<long>;

public class CreateSaleCommandHandler(
    IAppDbContext context,
    IMapper mapper) : IRequestHandler<CreateSaleCommand, long>
{
    public async Task<long> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        await context.BeginTransactionAsync(cancellationToken);

        try
        {
            var user = await context.Users
                .Include(a => a.Accounts)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
                ?? throw new NotFoundException(nameof(User), nameof(request.UserId), request.UserId);

            var productResidues = await context.ProductResidues
                .Include(p => p.ProductEntries)
                .Where(p => request.SaleItems.Select(i => i.ProductTypeId).Contains(p.Id))
                .ToListAsync(cancellationToken);

            decimal costPrice = 0;

            List<SaleItem> saleItems = new List<SaleItem>();

            foreach (var item in request.SaleItems)
            {
                var residue = productResidues.FirstOrDefault(r => r.ProductTypeId == item.ProductTypeId)
                    ?? throw new NotFoundException(nameof(ProductResidue), nameof(item.ProductTypeId), item.ProductTypeId);

                residue.TypeCount -= item.TypeCount;
                var saleItem = mapper.Map<SaleItem>(item);
                saleItem.CostPrice = residue.ProductEntries.Last().CostPrice;
            }

            var sale = mapper.Map<Sale>(request);
            sale.CostPrice = costPrice;
            sale.BenifitPrice = 0;



            context.Sales.Add(sale);


            await context.CommitTransactionAsync(cancellationToken);
            return sale.Id;
        }
        catch
        {
            await context.RollbackTransactionAsync(cancellationToken);

        }
        return 0;
    }
}
