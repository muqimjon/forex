namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class Cash : Auditable
{
    public int ShopId { get; set; }
    public int CurrencyId { get; set; }
    public decimal Balance { get; set; }

    public Shop Shop { get; set; } = default!;
    public Currency Currency { get; set; } = default!;
}