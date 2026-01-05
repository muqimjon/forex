namespace Forex.Application.Features.UnitMeasures.Queries;

using AutoMapper;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Common.Models;
using Forex.Application.Features.UnitMeasures.DTOs;
using MediatR;

public record UnitMeasureFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<UnitMeasureDto>>;

public class UnitMeasureFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<UnitMeasureFilterQuery, IReadOnlyCollection<UnitMeasureDto>>
{
    public async Task<IReadOnlyCollection<UnitMeasureDto>> Handle(UnitMeasureFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<UnitMeasureDto>>(await context.UnitMeasures
            .ToPagedListAsync(request, writer, cancellationToken));
}