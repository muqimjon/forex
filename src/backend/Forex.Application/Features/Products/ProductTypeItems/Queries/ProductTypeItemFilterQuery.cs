namespace Forex.Application.Features.Products.ProductTypeItems.Queries;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Commons.Models;
using Forex.Application.Features.Products.ProductTypeItems.DTOs;
using MediatR;

public record ProductTypeItemFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<ProductTypeItemDto>>;

public class ProductTypeItemFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<ProductTypeItemFilterQuery, IReadOnlyCollection<ProductTypeItemDto>>
{
    public async Task<IReadOnlyCollection<ProductTypeItemDto>> Handle(ProductTypeItemFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<ProductTypeItemDto>>(await context.ProductTypeItems
            .ToPagedListAsync(request, writer, cancellationToken));
}
