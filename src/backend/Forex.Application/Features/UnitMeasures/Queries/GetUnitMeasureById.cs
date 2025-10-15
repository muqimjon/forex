namespace Forex.Application.Features.UnitMeasures.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.UnitMeasures.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetUnitMeasureByIdQuery(long Id) : IRequest<UnitMeasureDto>;

public class GetUnitMeasureByIdQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetUnitMeasureByIdQuery, UnitMeasureDto>
{
    public async Task<UnitMeasureDto> Handle(GetUnitMeasureByIdQuery request, CancellationToken cancellationToken)
        => mapper.Map<UnitMeasureDto>(await context.Manufactories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken));
}
