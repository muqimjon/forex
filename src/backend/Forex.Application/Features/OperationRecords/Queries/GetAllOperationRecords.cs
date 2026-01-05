namespace Forex.Application.Features.OperationRecords.Queries;

using AutoMapper;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.OperationRecords.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllOperationRecordsQuery : IRequest<IReadOnlyCollection<OperationRecordDto>>;

public class GetAllOperationRecordsQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllOperationRecordsQuery, IReadOnlyCollection<OperationRecordDto>>
{
    public async Task<IReadOnlyCollection<OperationRecordDto>> Handle(GetAllOperationRecordsQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<OperationRecordDto>>(await context.OperationRecords.AsNoTracking()
            .Include(or => or.Sale)
            .Include(or => or.Transaction)
            .ToListAsync(cancellationToken));
}
