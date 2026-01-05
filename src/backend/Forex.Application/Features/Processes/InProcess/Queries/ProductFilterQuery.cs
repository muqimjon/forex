namespace Forex.Application.Features.Processes.InProcess.Queries;

using AutoMapper;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Common.Models;
using Forex.Application.Features.Processes.InProcess.DTOs;
using MediatR;

public record InProcessFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<InProcessDto>>;

public class InProcessFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<InProcessFilterQuery, IReadOnlyCollection<InProcessDto>>
{
    public async Task<IReadOnlyCollection<InProcessDto>> Handle(InProcessFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<InProcessDto>>(await context.InProcesses
            .ToPagedListAsync(request, writer, cancellationToken));
}
