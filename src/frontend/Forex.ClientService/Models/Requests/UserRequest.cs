namespace Forex.ClientService.Models.Requests;

using Forex.ClientService.Enums;

public sealed record UserRequest
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public List<UserAccount> Accounts { get; set; } = default!;
}
