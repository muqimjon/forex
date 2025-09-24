namespace Forex.Domain.Entities.Shops;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Manufactories;

public class ProductTypeItem : Auditable
{
    public long ProductTypeId { get; set; }
    public long SemiProductId { get; set; }
    public decimal Quantity { get; set; }
    public int SemiProductCode { get; set; }

    public ProductType ProductPype { get; set; } = default!;
    public SemiProduct SemiProduct { get; set; } = default!;
}