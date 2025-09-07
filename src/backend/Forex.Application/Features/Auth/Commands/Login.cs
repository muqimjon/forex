namespace Forex.Application.Features.Auth.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record LoginCommand(
    string EmailOrPhone,
    string Password)
    : IRequest<string>;

public class LoginCommandHandler(
    IAppDbContext context,
    IPasswordHasher hasher,
    IJwtTokenGenerator jwt)
    : IRequestHandler<LoginCommand, string>
{
    public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(
            u => u.Phone == request.EmailOrPhone || u.Email == request.EmailOrPhone,
            cancellationToken);

        if (user is null || !hasher.VerifyPassword(user.PasswordHash!, request.Password))
            throw new ConflictException("Login yoki parol noto‘g‘ri.");

        var roles = new List<string> { user.Role.ToString() };

        return jwt.GenerateToken(user, roles);
    }
}
