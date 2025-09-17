namespace Forex.Application.Features.Shops.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Shops.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record GetAllShopsQuery : IRequest<IReadOnlyCollection<ShopDto>>;

public class GetAllShopsQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllShopsQuery, IReadOnlyCollection<ShopDto>>
{
    public async Task<IReadOnlyCollection<ShopDto>> Handle(GetAllShopsQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<ShopDto>>(await context.Shops.AsNoTracking().ToListAsync(cancellationToken));
}
