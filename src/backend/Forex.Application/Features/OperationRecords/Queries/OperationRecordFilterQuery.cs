namespace Forex.Application.Features.OperationRecords.Queries;

using AutoMapper;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Common.Models;
using Forex.Application.Features.OperationRecords.DTOs;
using MediatR;

public record OperationRecordFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<OperationRecordDto>>;

public class OperationRecordFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<OperationRecordFilterQuery, IReadOnlyCollection<OperationRecordDto>>
{
    public async Task<IReadOnlyCollection<OperationRecordDto>> Handle(OperationRecordFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<OperationRecordDto>>(await context.OperationRecords
            .ToPagedListAsync(request, writer, cancellationToken));
}
