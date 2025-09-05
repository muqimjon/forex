namespace Forex.Application.Features.Currencies.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Currencies.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record GetAllCurrenciesQuery : IRequest<List<CurrencyDto>>;

public class GetAllCurrenciesQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllCurrenciesQuery, List<CurrencyDto>>
{
    public async Task<List<CurrencyDto>> Handle(GetAllCurrenciesQuery request, CancellationToken cancellationToken)
        => mapper.Map<List<CurrencyDto>>(await context.Currencies.AsNoTracking().ToListAsync(cancellationToken));
}
