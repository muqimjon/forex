namespace Forex.Application.Features.Products.Products.Queries;

using AutoMapper;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Common.Models;
using Forex.Application.Features.Products.Products.DTOs;
using MediatR;

public record ProductFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<ProductDto>>;

public class ProductFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<ProductFilterQuery, IReadOnlyCollection<ProductDto>>
{
    public async Task<IReadOnlyCollection<ProductDto>> Handle(ProductFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<ProductDto>>(await context.Products
            .ToPagedListAsync(request, writer, cancellationToken));
}
