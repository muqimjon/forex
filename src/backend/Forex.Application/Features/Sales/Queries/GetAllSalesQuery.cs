namespace Forex.Application.Features.Sales.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Sales.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllSalesQuery : IRequest<IReadOnlyCollection<SaleDto>>;

public class GetAllSalesQueryHandler(
    IAppDbContext context,
    IMapper mapper) : IRequestHandler<GetAllSalesQuery, IReadOnlyCollection<SaleDto>>
{
    public async Task<IReadOnlyCollection<SaleDto>> Handle(GetAllSalesQuery request, CancellationToken cancellationToken)
    { 
       var a = mapper.Map<IReadOnlyCollection<SaleDto>>(await context.Sales
        .Include(s => s.Customer)
        .Include(items => items.SaleItems)
        .ThenInclude(pt => pt.ProductType)
        .ThenInclude(p => p.Product)
        .ThenInclude(m => m.UnitMeasure)
        .AsNoTracking()
        .ToListAsync(cancellationToken));
        return a;
    }
}