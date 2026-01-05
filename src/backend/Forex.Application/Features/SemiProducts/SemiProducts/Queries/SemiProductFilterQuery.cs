namespace Forex.Application.Features.SemiProducts.SemiProducts.Queries;

using AutoMapper;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Common.Models;
using Forex.Application.Features.SemiProducts.SemiProducts.DTOs;
using MediatR;

public record SemiProductFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<SemiProductDto>>;

public class SemiProductFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<SemiProductFilterQuery, IReadOnlyCollection<SemiProductDto>>
{
    public async Task<IReadOnlyCollection<SemiProductDto>> Handle(SemiProductFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<SemiProductDto>>(await context.SemiProducts
            .ToPagedListAsync(request, writer, cancellationToken));
}