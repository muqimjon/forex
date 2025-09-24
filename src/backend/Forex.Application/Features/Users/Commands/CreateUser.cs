namespace Forex.Application.Features.Users.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Users;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record CreateUserCommand(
    string Name,
    string Phone,
    string? Email,
    Role Role,
    string Address,
    decimal Balance,
    string Description)
    : IRequest<long>;

public class CreateUserCommandHandler(
    IAppDbContext context,
    IMapper mapper) : IRequestHandler<CreateUserCommand, long>
{
    public async Task<long> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var isExist = await context.Users
            .AnyAsync(user => user.Phone == request.Phone, cancellationToken);

        if (isExist)
            throw new AlreadyExistException(nameof(User), nameof(request.Phone), request.Phone);

        var user = mapper.Map<User>(request);
        context.Users.Add(user);
        context.UserAccounts.Add(new()
        {
            OpeningBalance = request.Balance,
            Balance = request.Balance,
            User = user,
        });

        await context.SaveAsync(cancellationToken);
        return user.Id;
    }
}
