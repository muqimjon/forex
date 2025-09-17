namespace Forex.Application.Features.SemiProducts.DTOs;

public sealed record SemiProductDto
{
    public long Id { get; init; }
    public string? Name { get; init; }
    public int Code { get; init; }
    public string Measure { get; init; } = string.Empty;
    public string? PhotoPath { get; init; }
}
