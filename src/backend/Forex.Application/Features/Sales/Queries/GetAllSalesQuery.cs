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
    => mapper.Map<IReadOnlyCollection<SaleDto>>(await context.Sales
        .Include(s => s.Customer)
        .AsQueryable()
        .ToListAsync(cancellationToken));
}