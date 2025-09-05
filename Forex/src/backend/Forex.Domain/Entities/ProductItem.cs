namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class ProductItem : Auditable
{
    public long ProductId { get; set; }
    public long SemiProductId { get; set; }
    public decimal Quantity { get; set; }

    public Product Product { get; set; } = default!;
    public SemiProduct SemiProduct { get; set; } = default!;
}