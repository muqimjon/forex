namespace Forex.Application.Features.Products.Queries;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Commons.Models;
using Forex.Application.Features.SemiProducts.DTOs;
using MediatR;

public record ProductFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<SemiProductDto>>;

public class ProductFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<ProductFilterQuery, IReadOnlyCollection<SemiProductDto>>
{
    public async Task<IReadOnlyCollection<SemiProductDto>> Handle(ProductFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<SemiProductDto>>(await context.SemiProducts
            .ToPagedListAsync(request, writer, cancellationToken));
}
