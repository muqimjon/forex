namespace Forex.Domain.Entities.Products;

using Forex.Domain.Commons;
using Forex.Domain.Entities.SemiProducts;

public class ProductTypeItem : Auditable
{
    public decimal Quantity { get; set; }   // miqdori

    public long TypeId { get; set; }
    public ProductType Type { get; set; } = default!;

    public long SemiProductId { get; set; }
    public SemiProduct SemiProduct { get; set; } = default!;
}