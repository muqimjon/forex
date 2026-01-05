namespace Forex.Application.Features.Products.ProductResidues.Queries;

using AutoMapper;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Common.Models;
using Forex.Application.Features.Products.ProductResidues.DTOs;
using MediatR;

public record ProductResidueFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<ProductResidueDto>>;

public class ProductResidueFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<ProductResidueFilterQuery, IReadOnlyCollection<ProductResidueDto>>
{
    public async Task<IReadOnlyCollection<ProductResidueDto>> Handle(ProductResidueFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<ProductResidueDto>>(await context.ProductResidues
            .ToPagedListAsync(request, writer, cancellationToken));
}
