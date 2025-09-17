namespace Forex.Application.Features.SemiProducts.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.SemiProducts.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record GetAllSemiProductsQuery : IRequest<IReadOnlyCollection<SemiProductDto>>;

public class GetAllSemiProductsQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllSemiProductsQuery, IReadOnlyCollection<SemiProductDto>>
{
    public async Task<IReadOnlyCollection<SemiProductDto>> Handle(GetAllSemiProductsQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<SemiProductDto>>(await context.SemiProducts.AsNoTracking().ToListAsync(cancellationToken));
}
