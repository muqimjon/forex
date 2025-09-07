namespace Forex.Application.Features.Auth.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Users;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record RegisterCommand(
    string Name,
    string Phone,
    string? Email,
    Role Role,
    string? Address,
    string? Description,
    string Password)
    : IRequest<string>;

public class RegisterCommandHandler(
    IAppDbContext context,
    IMapper mapper,
    IPasswordHasher hasher,
    IJwtTokenGenerator jwt)
    : IRequestHandler<RegisterCommand, string>
{
    public async Task<string> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var exists = await context.Users
            .AnyAsync(u => u.Phone == request.Phone || u.Email == request.Email, cancellationToken);

        if (exists)
            throw new AlreadyExistException(nameof(User));

        var user = mapper.Map<User>(request);
        user.PasswordHash = hasher.HashPassword(request.Password);

        context.Users.Add(user);
        await context.SaveAsync(cancellationToken);

        var roles = new List<string> { user.Role.ToString() };
        var token = jwt.GenerateToken(user, roles);

        return token;
    }
}
