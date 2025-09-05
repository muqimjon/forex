namespace Forex.Application.Features.ShopCashes.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.ShopCashes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record GetShopCashByIdQuery(long Id) : IRequest<ShopCashDto>;

public class GetShopCashByIdQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetShopCashByIdQuery, ShopCashDto>
{
    public async Task<ShopCashDto> Handle(GetShopCashByIdQuery request, CancellationToken cancellationToken)
        => mapper.Map<ShopCashDto>(await context.ShopCashes
            .Include(c => c.Currency)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken));
}
