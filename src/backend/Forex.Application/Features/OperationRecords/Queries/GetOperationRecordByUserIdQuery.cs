namespace Forex.Application.Features.OperationRecords.Queries;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.OperationRecords.DTOs;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;


public record GetOperationRecordByUserIdQuery(
    long UserId,
    DateTime Begin,
    DateTime End) : IRequest<OperationRecordTurnoverDto>;

public class GetOperationRecordByUserIdQueryHandler(IAppDbContext _context, IMapper mapper)
    : IRequestHandler<GetOperationRecordByUserIdQuery, OperationRecordTurnoverDto>
{

    public async Task<OperationRecordTurnoverDto> Handle(
        GetOperationRecordByUserIdQuery request,
        CancellationToken ct)
    {
        var openingBalance = await GetOpeningBalanceAsync(request.UserId, ct);

        var allRecords = await GetAllUserOperationRecordsAsync(request.UserId, ct);

        var beginBalance = CalculateBalance(openingBalance, allRecords, request.Begin, isEndDate: false);
        var endBalance = CalculateBalance(openingBalance, allRecords, request.End, isEndDate: true);

        var operationsInRange = mapper.Map<List<OperationRecordDto>>(
        FilterOperationRecordsInRange(allRecords, request.Begin, request.End)
        );

        return new OperationRecordTurnoverDto
        {
            BeginBalance = beginBalance,
            EndBalance = endBalance,
            OperationRecords = operationsInRange
        };
    }

    private async Task<decimal> GetOpeningBalanceAsync(long userId, CancellationToken ct)
    {
        var account = await _context.UserAccounts
            .FirstOrDefaultAsync(a => a.UserId == userId, ct)
            ?? throw new NotFoundException("UserAccount", nameof(userId), userId);

        return account.OpeningBalance;
    }

    private async Task<List<OperationRecord>> GetAllUserOperationRecordsAsync(long userId, CancellationToken ct)
    {
        return await _context.OperationRecords
            .Include(x => x.Sale)
            .Include(x => x.Transaction)
            .Where(or =>
                (or.Sale != null && or.Sale.CustomerId == userId) ||
                (or.Transaction != null && or.Transaction.UserId == userId)
            )
            .ToListAsync(ct);
    }

    private static decimal CalculateBalance(decimal openingBalance, List<OperationRecord> all, DateTime date, bool isEndDate)
    {
        var turnover = all
            .Where(or => isEndDate ? or.Date <= date.ToUtcSafe() : or.Date < date.ToUtcSafe())
            .Sum(or => or.Amount);

        return openingBalance + turnover;
    }

    private static List<OperationRecord> FilterOperationRecordsInRange(
        List<OperationRecord> all,
        DateTime begin,
        DateTime end) => [.. all
            .Where(or => or.Date >= begin.ToUtcSafe() && or.Date <= end.AddDays(1).ToUtcSafe())
            .OrderBy(or => or.Date)];
}
