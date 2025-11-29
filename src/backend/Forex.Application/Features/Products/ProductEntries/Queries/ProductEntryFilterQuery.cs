namespace Forex.Application.Features.Products.ProductEntries.Queries;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Commons.Models;
using Forex.Application.Features.Products.ProductEntries.DTOs;
using MediatR;

public record ProductEntryFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<ProductEntryDto>>;

public class ProductEntryFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<ProductEntryFilterQuery, IReadOnlyCollection<ProductEntryDto>>
{
    public async Task<IReadOnlyCollection<ProductEntryDto>> Handle(ProductEntryFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<ProductEntryDto>>(await context.ProductEntries
            .ToPagedListAsync(request, writer, cancellationToken));
}
