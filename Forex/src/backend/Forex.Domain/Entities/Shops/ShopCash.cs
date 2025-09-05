namespace Forex.Domain.Entities.Shops;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Payments;

public class ShopCash : Auditable
{
    public long ShopId { get; set; }
    public long CurrencyId { get; set; }
    public decimal Balance { get; set; }

    public Shop Shop { get; set; } = default!;
    public Currency Currency { get; set; } = default!;
}