namespace Forex.Application.Features.Auth.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Auth.DTOs;
using Forex.Application.Features.Users.DTOs;
using Forex.Domain.Entities;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record RegisterCommand(
    string Name,
    string Phone,
    string? Email,
    UserRole Role,
    string? Address,
    string? Description,
    string Password)
    : IRequest<AuthResponse>;

public class RegisterCommandHandler(
    IAppDbContext context,
    IMapper mapper,
    IPasswordHasher hasher,
    IJwtTokenGenerator jwt)
    : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var exists = await context.Users
            .AnyAsync(u => !string.IsNullOrEmpty(request.Phone) && u.Phone == request.Phone ||
                !string.IsNullOrEmpty(request.Email) && u.Email == request.Email, cancellationToken);

        if (exists)
            throw new AlreadyExistException(nameof(User));

        var user = mapper.Map<User>(request);
        user.PasswordHash = hasher.HashPassword(request.Password);
        Console.WriteLine($"[DEBUG] DBdagi hash: {user.PasswordHash}");

        context.Users.Add(user);
        await context.SaveAsync(cancellationToken);

        var roles = new List<string> { user.Role.ToString() };
        var token = jwt.GenerateToken(user, roles);

        return new()
        {
            Token = jwt.GenerateToken(user, roles),
            User = mapper.Map<UserDto>(user),
        };
    }
}
