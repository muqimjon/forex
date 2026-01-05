namespace Forex.Application.Features.Users.Queries;

using AutoMapper;
using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.Users.DTOs;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetUserByIdQuery(long Id) : IRequest<UserDto>;

public class GetUserByIdQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        => mapper.Map<UserDto>(await context.Users
            .Include(user => user.Accounts)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken))
        ?? throw new NotFoundException(nameof(User), nameof(request.Id), request.Id);
}
