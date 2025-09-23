namespace Forex.Application.Features.Manufactories.Queries;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Commons.Models;
using Forex.Application.Features.Manufactories.DTOs;
using MediatR;

public record ManufactoryFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<ManufactoryDto>>;

public class ManufactoryFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<ManufactoryFilterQuery, IReadOnlyCollection<ManufactoryDto>>
{
    public async Task<IReadOnlyCollection<ManufactoryDto>> Handle(ManufactoryFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<ManufactoryDto>>(await context.Manufactories
            .ToPagedListAsync(request, writer, cancellationToken));
}