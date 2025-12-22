namespace Forex.ClientService.Models.Requests;

public sealed record LoginRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}
