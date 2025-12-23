namespace Forex.Application.Features.Auth.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Auth.DTOs;
using Forex.Application.Features.Users.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record LoginCommand(
    string? Username,
    string? Password)
    : IRequest<AuthResponse>;

public class LoginCommandHandler(
    IAppDbContext context,
    IMapper mapper,
    IPasswordHasher hasher,
    IJwtTokenGenerator jwt)
    : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(
            u =>  u.Username == request.Username,
            cancellationToken);

        if (user is null || !hasher.VerifyPassword(user.PasswordHash!, request.Password!))
            throw new ConflictException("Login yoki parol noto‘g‘ri.");

        var roles = new List<string> { user.Role.ToString() };

        return new()
        {
            Token = jwt.GenerateToken(user, roles),
            User = mapper.Map<UserDto>(user),
        };
    }
}