namespace Forex.Domain.Entities.Shops;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Manufactories;

public class ProductItem : Auditable
{
    public long ProductId { get; set; }
    public long SemiProductId { get; set; }
    public decimal Quantity { get; set; }

    public Product Product { get; set; } = default!;
    public SemiProduct SemiProduct { get; set; } = default!;
}