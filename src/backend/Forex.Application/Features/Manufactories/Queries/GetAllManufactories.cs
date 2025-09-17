namespace Forex.Application.Features.Manufactories.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Manufactories.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record GetAllManufactoriesQuery : IRequest<List<ManufactoryDto>>;

public class GetAllManufactoriesQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllManufactoriesQuery, List<ManufactoryDto>>
{
    public async Task<List<ManufactoryDto>> Handle(GetAllManufactoriesQuery request, CancellationToken cancellationToken)
        => mapper.Map<List<ManufactoryDto>>(await context.Manufactories
            .Include(m => m.SemiProductResidues)
                .ThenInclude(spr => spr.SemiProduct)
            .AsNoTracking().ToListAsync(cancellationToken));
}
