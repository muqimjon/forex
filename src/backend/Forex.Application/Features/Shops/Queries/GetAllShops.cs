namespace Forex.Application.Features.Shops.Queries;

using AutoMapper;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.Shops.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllShopsQuery : IRequest<IReadOnlyCollection<ShopDto>>;

public class GetAllShopsQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllShopsQuery, IReadOnlyCollection<ShopDto>>
{
    public async Task<IReadOnlyCollection<ShopDto>> Handle(GetAllShopsQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<ShopDto>>(await context.Shops
            .Include(sh => sh.ShopAccounts)
                .ThenInclude(sa => sa.Currency)
            .AsNoTracking()
            .ToListAsync(cancellationToken));
}
