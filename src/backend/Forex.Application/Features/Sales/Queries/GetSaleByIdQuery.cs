namespace Forex.Application.Features.Sales.Queries;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Sales.DTOs;
using Forex.Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetSaleByIdQuery(long Id) : IRequest<SaleDto>;

public class GetSaleByIdQueryHandler(
    IAppDbContext context) : IRequestHandler<GetSaleByIdQuery, SaleDto>
{
    public async Task<SaleDto> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var sale = await context.Sales
            .Include(s => s.SaleItems)
                .ThenInclude(i => i.ProductType)
            .Include(s => s.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Sale), nameof(request.Id), request.Id);

        var dto = new SaleDto
        {
            Id = sale.Id,
            Date = sale.CreatedAt,
            TotalAmount = sale.TotalAmount,
            UserName = sale.User.Name,
            Items = sale.SaleItems.Select(i => new SaleItemDto
            {
                Id = i.Id,
                ProductTypeId = i.ProductTypeId,
                TypeCount = i.TypeCount,
                Price = i.CostPrice + i.Benifit
            }).ToList()
        };

        return dto;
    }
}
