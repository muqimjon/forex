namespace Forex.Application.Features.Users.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Users.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllUsersQuery : IRequest<IReadOnlyCollection<UserDto>>;

public class GetAllUsersQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllUsersQuery, IReadOnlyCollection<UserDto>>
{
    public async Task<IReadOnlyCollection<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<UserDto>>(await context.Users.AsNoTracking()
            .Where(u => !u.IsDeleted)
            .ToListAsync(cancellationToken));
}
