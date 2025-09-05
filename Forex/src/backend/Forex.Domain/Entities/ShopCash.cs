namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class ShopCash : Auditable
{
    public long ShopId { get; set; }
    public long CurrencyId { get; set; }
    public decimal Balance { get; set; }

    public Shop Shop { get; set; } = default!;
    public Currency Currency { get; set; } = default!;
}