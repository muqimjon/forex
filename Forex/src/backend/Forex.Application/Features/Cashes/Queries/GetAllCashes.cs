namespace Forex.Application.Features.Cashes.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Cashes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record GetAllCashesQuery : IRequest<List<CashDto>>;

public class GetAllCashesQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllCashesQuery, List<CashDto>>
{
    public async Task<List<CashDto>> Handle(GetAllCashesQuery request, CancellationToken cancellationToken)
        => mapper.Map<List<CashDto>>(await context.Cashes.AsNoTracking().ToListAsync(cancellationToken));
}
