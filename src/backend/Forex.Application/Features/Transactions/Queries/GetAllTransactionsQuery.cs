namespace Forex.Application.Features.Transactions.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Transactions.DTOs;
using Forex.Application.Features.Transactions.DTOs;
using Forex.Application.Features.Transactions.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public record GetAllTransactionsQuery : IRequest<IReadOnlyCollection<TransactionDto>>;

public class GetAllTransactionsQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllTransactionsQuery, IReadOnlyCollection<TransactionDto>>
{
    public async Task<IReadOnlyCollection<TransactionDto>> Handle(GetAllTransactionsQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<TransactionDto>>(await context.Transactions
            .Include(t => t.User)           
            .Include(t => t.Currency)       
            .AsNoTracking()
            .ToListAsync(cancellationToken));
}