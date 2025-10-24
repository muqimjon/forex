namespace Forex.ClientService.Models.Requests;

public sealed record LoginRequest
{
    public string EmailOrPhone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
