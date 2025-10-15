namespace Forex.ClientService.Models.Responses;

public sealed record SemiProductResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Code { get; set; }
    public string Measure { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
}
