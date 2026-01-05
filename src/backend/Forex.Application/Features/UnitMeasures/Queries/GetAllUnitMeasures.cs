namespace Forex.Application.Features.UnitMeasures.Queries;

using AutoMapper;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.UnitMeasures.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllUnitMeasuresQuery : IRequest<IReadOnlyCollection<UnitMeasureDto>>;

public class GetAllUnitMeasuresQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllUnitMeasuresQuery, IReadOnlyCollection<UnitMeasureDto>>
{
    public async Task<IReadOnlyCollection<UnitMeasureDto>> Handle(GetAllUnitMeasuresQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<UnitMeasureDto>>(await context.UnitMeasures
            .AsNoTracking()
            .ToListAsync(cancellationToken));
}
