namespace Forex.ClientService.Models.Responses;

using Forex.ClientService.Enums;
using System.Text.Json.Serialization;

public sealed record UserResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IList<UserAccountResponse> Accounts { get; set; } = default!;

    [JsonIgnore]
    public bool IsEditing { get; set; }
}