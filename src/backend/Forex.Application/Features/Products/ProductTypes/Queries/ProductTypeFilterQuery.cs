namespace Forex.Application.Features.Products.ProductTypes.Queries;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Commons.Models;
using Forex.Application.Features.Products.ProductTypes.DTOs;
using MediatR;

public record ProductTypeFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<ProductTypeDto>>;

public class ProductTypeFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<ProductTypeFilterQuery, IReadOnlyCollection<ProductTypeDto>>
{
    public async Task<IReadOnlyCollection<ProductTypeDto>> Handle(ProductTypeFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<ProductTypeDto>>(await context.ProductTypes
            .ToPagedListAsync(request, writer, cancellationToken));
}
