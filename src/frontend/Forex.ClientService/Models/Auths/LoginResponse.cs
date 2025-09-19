namespace Forex.ClientService.Models.Auths;

using Forex.ClientService.Models.Users;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserResponse User { get; set; } = default!;
}