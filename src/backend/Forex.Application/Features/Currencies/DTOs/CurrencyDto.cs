namespace Forex.Application.Features.Currencies.DTOs;

public record CurrencyDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NormalizedName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public bool IsEditable { get; set; }
    public int Position { get; set; }
}