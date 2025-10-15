namespace Forex.Application.Features.Currencies.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Currencies.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetCurrencyByIdQuery(long Id) : IRequest<CurrencyDto>;

public class GetCurrencyByIdQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetCurrencyByIdQuery, CurrencyDto>
{
    public async Task<CurrencyDto> Handle(GetCurrencyByIdQuery request, CancellationToken cancellationToken)
        => mapper.Map<CurrencyDto>(await context.Currencies
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken));
}
