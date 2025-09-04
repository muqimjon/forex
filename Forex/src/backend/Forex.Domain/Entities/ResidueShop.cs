namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class ResidueShop : Auditable
{
    public int ProductId { get; set; }
    public int ShopId { get; set; }
    public decimal Quantity { get; set; }

    public Product Product { get; set; } = default!;
    public Shop Shop { get; set; } = default!;
}