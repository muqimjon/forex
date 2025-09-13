namespace Forex.ClientService.Models.Users;

using Forex.ClientService.Enums;

public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Role Role { get; set; }
    public decimal Balance { get; set; }
}