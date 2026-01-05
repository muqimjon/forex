namespace Forex.Application.Features.Users.Queries;

using AutoMapper;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Common.Models;
using Forex.Application.Features.Users.DTOs;
using MediatR;

public record UserFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<UserDto>>;

public class UserFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<UserFilterQuery, IReadOnlyCollection<UserDto>>
{
    public async Task<IReadOnlyCollection<UserDto>> Handle(UserFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<UserDto>>(await context.Users
            .ToPagedListAsync(request, writer, cancellationToken));
}

