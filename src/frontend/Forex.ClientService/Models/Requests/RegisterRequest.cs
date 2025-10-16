namespace Forex.ClientService.Models.Requests;

public sealed record RegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string Password { get; set; } = string.Empty;
    public string? Description { get; set; }
}