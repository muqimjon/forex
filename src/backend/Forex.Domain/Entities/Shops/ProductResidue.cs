namespace Forex.Domain.Entities.Shops;

using Forex.Domain.Commons;

public class ProductResidue : Auditable
{
    public long ProductId { get; set; }
    public long ShopId { get; set; }
    public decimal Quantity { get; set; }

    public Product Product { get; set; } = default!;
    public Shop Shop { get; set; } = default!;
}