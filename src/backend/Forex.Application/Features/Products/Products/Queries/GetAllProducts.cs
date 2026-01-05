namespace Forex.Application.Features.Products.Products.Queries;

using AutoMapper;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.Products.Products.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllProductsQuery : IRequest<IReadOnlyCollection<ProductDto>>;

public class GetAllProductsQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllProductsQuery, IReadOnlyCollection<ProductDto>>
{
    public async Task<IReadOnlyCollection<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<ProductDto>>(await context.Products.AsNoTracking()
            .Include(p => p.UnitMeasure)
            .Include(p => p.ProductTypes)
            .ThenInclude(pt => pt.ProductTypeItems)
            .ThenInclude(pti => pti.SemiProduct)
            .ToListAsync(cancellationToken));
}
