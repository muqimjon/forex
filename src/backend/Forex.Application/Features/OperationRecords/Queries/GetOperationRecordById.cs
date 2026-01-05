namespace Forex.Application.Features.OperationRecords.Queries;

using AutoMapper;
using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.OperationRecords.DTOs;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetOperationRecordByIdQuery(long Id) : IRequest<OperationRecordDto>;

public class GetOperationRecordByIdQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetOperationRecordByIdQuery, OperationRecordDto>
{
    public async Task<OperationRecordDto> Handle(GetOperationRecordByIdQuery request, CancellationToken cancellationToken)
        => mapper.Map<OperationRecordDto>(await context.OperationRecords
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken))
        ?? throw new NotFoundException(nameof(OperationRecord), nameof(request.Id), request.Id);
}
