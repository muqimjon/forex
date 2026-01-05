namespace Forex.Application.Features.Sales.Queries;

using AutoMapper;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Common.Models;
using Forex.Application.Features.Sales.DTOs;
using MediatR;

public record SaleFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<SaleDto>>;

public class SaleFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<SaleFilterQuery, IReadOnlyCollection<SaleDto>>
{
    public async Task<IReadOnlyCollection<SaleDto>> Handle(SaleFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<SaleDto>>(await context.Sales
            .ToPagedListAsync(request, writer, cancellationToken));
}