namespace Forex.Application.Features.Currencies.Queries;

using AutoMapper;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Common.Models;
using Forex.Application.Features.Currencies.DTOs;
using MediatR;

public record CurrencyFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<CurrencyDto>>;

public class CurrencyFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<CurrencyFilterQuery, IReadOnlyCollection<CurrencyDto>>
{
    public async Task<IReadOnlyCollection<CurrencyDto>> Handle(CurrencyFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<CurrencyDto>>(await context.Currencies
            .ToPagedListAsync(request, writer, cancellationToken));
}