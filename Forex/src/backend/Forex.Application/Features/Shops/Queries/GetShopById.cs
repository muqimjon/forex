namespace Forex.Application.Features.Shops.Queries;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Shops.DTOs;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record GetShopByIdQuery(long Id) : IRequest<ShopDto>;

public class GetShopByIdQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetShopByIdQuery, ShopDto>
{
    public async Task<ShopDto> Handle(GetShopByIdQuery request, CancellationToken cancellationToken)
        => mapper.Map<ShopDto>(await context.Shops
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken))
        ?? throw new NotFoundException(nameof(Shop), nameof(request.Id), request.Id);
}
