namespace Forex.Application.Features.Transactions.Queries;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Commons.Models;
using Forex.Application.Features.Transactions.DTOs;
using MediatR;
using System.Collections.Generic;

public record TransactionFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<TransactionDto>>;

public class TransactionFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<TransactionFilterQuery, IReadOnlyCollection<TransactionDto>>
{
    public async Task<IReadOnlyCollection<TransactionDto>> Handle(TransactionFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<TransactionDto>>(await context.Transactions
            .ToPagedListAsync(request, writer, cancellationToken));
}