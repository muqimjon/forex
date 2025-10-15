namespace Forex.ClientService.Models.Responses;

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
    public List<Account> Accounts { get; set; } = default!;

    public bool IsEditing { get; set; }
}