namespace Forex.Application.Features.Users.Queries;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Commons.Models;
using Forex.Application.Features.Users.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UserFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<UserDto>>;

public class UserFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<UserFilterQuery, IReadOnlyCollection<UserDto>>
{
    public async Task<IReadOnlyCollection<UserDto>> Handle(UserFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<UserDto>>(await context.Users
            .Include(u => u.Account)
            .ToPagedListAsync(request, cancellationToken));
}
