namespace Forex.Application.Features.Users.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Users.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllUsersQuery : IRequest<List<UserDto>>;

public class GetAllUsersQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        => mapper.Map<List<UserDto>>(await context.Users.AsNoTracking().ToListAsync(cancellationToken));
}
