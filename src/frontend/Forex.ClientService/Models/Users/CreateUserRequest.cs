namespace Forex.ClientService.Models.Users;

using Forex.ClientService.Enums;

public class CreateUserRequest
{
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string Description { get; set; }
    public Role Role { get; set; }
    public decimal Balance { get; set; }
}