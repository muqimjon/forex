namespace Forex.ClientService.Models.Currencies;

public class CurrencyDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
