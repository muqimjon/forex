namespace Forex.Application.Features.Users.Queries;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Commons.Models;
using Forex.Application.Features.Users.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllFilteringUsersQuery : FilteringRequest, IRequest<List<UserDto>>;

public class GetAllFilteringUsersQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllFilteringUsersQuery, List<UserDto>>
{
    public async Task<List<UserDto>> Handle(GetAllFilteringUsersQuery request, CancellationToken cancellationToken)
        => mapper.Map<List<UserDto>>(await context.Users
            .AsNoTracking()
            .AsFilterable(request)
            .ToListAsync(cancellationToken));
}
