namespace Forex.ClientService.Models.Users;

using Forex.ClientService.Enums;

public sealed record UserResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Account Account { get; set; } = default!;
}