namespace Forex.Application.Features.ShopCashes.Queries;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Commons.Models;
using Forex.Application.Features.ShopCashes.DTOs;
using MediatR;

public record ShopCashFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<ShopCashDto>>;

public class ShopCashFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<ShopCashFilterQuery, IReadOnlyCollection<ShopCashDto>>
{
    public async Task<IReadOnlyCollection<ShopCashDto>> Handle(ShopCashFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<ShopCashDto>>(await context.ShopCashes
            .ToPagedListAsync(request, writer, cancellationToken));
}
