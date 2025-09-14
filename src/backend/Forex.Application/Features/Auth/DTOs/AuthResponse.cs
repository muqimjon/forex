namespace Forex.Application.Features.Auth.DTOs;

using Forex.Application.Features.Users.DTOs;

public record AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = default!;
}
