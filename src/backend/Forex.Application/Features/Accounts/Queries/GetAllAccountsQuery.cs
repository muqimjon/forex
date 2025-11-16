namespace Forex.Application.Features.Accounts.Queries;
using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Accounts.DTOs;
using Forex.Application.Features.Currencies.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public record GetAllAccountsQuery : IRequest<IReadOnlyCollection<UserAccountDto>>;

public class GetAllAccountsQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllAccountsQuery, IReadOnlyCollection<UserAccountDto>>
{
    public async Task<IReadOnlyCollection<UserAccountDto>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<UserAccountDto>>(await context.UserAccounts
            .Include(x => x.Currency)
            .Where(x => x.Balance < 0 )
            .ToListAsync(cancellationToken));
}
