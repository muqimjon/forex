namespace Forex.Application.Features.Currencies.DTOs;

public class CurrencyDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}