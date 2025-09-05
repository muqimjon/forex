namespace Forex.Application.Features.Cashes.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Cashes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record GetCashByIdQuery(long Id) : IRequest<CashDto>;

public class GetCashByIdQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetCashByIdQuery, CashDto>
{
    public async Task<CashDto> Handle(GetCashByIdQuery request, CancellationToken cancellationToken)
        => mapper.Map<CashDto>(await context.Cashes
            .Include(c => c.Currency)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken));
}
