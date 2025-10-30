namespace Forex.Application.Features.Currencies.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Currencies.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllCurrenciesQuery : IRequest<IReadOnlyCollection<CurrencyDto>>;

public class GetAllCurrenciesQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllCurrenciesQuery, IReadOnlyCollection<CurrencyDto>>
{
    public async Task<IReadOnlyCollection<CurrencyDto>> Handle(GetAllCurrenciesQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<CurrencyDto>>(await context.Currencies
            .OrderBy(c => c.Position)
            .ToListAsync(cancellationToken));
}
