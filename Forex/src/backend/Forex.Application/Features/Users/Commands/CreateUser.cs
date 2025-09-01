namespace Forex.Application.Features.Users.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record CreateUserCommand(
    string Name,
    string Phone,
    Role Role,
    string Address,
    string Description)
    : IRequest<long>;

public class CreateUserCommandHandler(
    IAppDbContext context,
    IMapper mapper) : IRequestHandler<CreateUserCommand, long>
{
    public async Task<long> Handle(CreateUserCommand request, CancellationToken cancellation)
    {
        var isExist = await context.Users
                    .AnyAsync(user => user.Phone == request.Phone, cancellation);

        if (isExist)
            throw new AlreadyExistException(nameof(User), nameof(request.Name), request.Name);

        var user = mapper.Map<User>(request);
        context.Users.Add(user);

        await context.SaveChangesAsync(cancellation);
        return user.Id;
    }
}
