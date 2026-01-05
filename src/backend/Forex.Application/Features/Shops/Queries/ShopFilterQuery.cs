namespace Forex.Application.Features.Shops.Queries;

using AutoMapper;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Common.Models;
using Forex.Application.Features.Shops.DTOs;
using MediatR;

public record ShopFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<ShopDto>>;

public class ShopFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<ShopFilterQuery, IReadOnlyCollection<ShopDto>>
{
    public async Task<IReadOnlyCollection<ShopDto>> Handle(ShopFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<ShopDto>>(await context.Shops
            .ToPagedListAsync(request, writer, cancellationToken));
}