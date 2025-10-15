namespace Forex.Domain.Entities.Payments;

using Forex.Domain.Commons;

public class Currency : Auditable
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? SearchName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}