namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class ProductItem : Auditable
{
    public int ProductId { get; set; }
    public int SemiProductId { get; set; }
    public decimal Quantity { get; set; }

    public Product Product { get; set; } = default!;
    public SemiProduct SemiProduct { get; set; } = default!;
}