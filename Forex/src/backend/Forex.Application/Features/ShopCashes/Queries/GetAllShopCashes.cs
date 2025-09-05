namespace Forex.Application.Features.ShopCashes.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.ShopCashes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record GetAllShopCashesQuery : IRequest<List<ShopCashDto>>;

public class GetAllShopCashesQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllShopCashesQuery, List<ShopCashDto>>
{
    public async Task<List<ShopCashDto>> Handle(GetAllShopCashesQuery request, CancellationToken cancellationToken)
        => mapper.Map<List<ShopCashDto>>(await context.ShopCashes.AsNoTracking().ToListAsync(cancellationToken));
}
