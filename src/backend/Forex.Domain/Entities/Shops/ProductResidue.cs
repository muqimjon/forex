namespace Forex.Domain.Entities.Shops;

using Forex.Domain.Commons;

public class ProductResidue : Auditable
{
    public long ProductTypeId { get; set; }
    public long ShopId { get; set; }
    public int TypeCount { get; set; }

    public ProductType ProductType { get; set; } = default!;
    public Shop Shop { get; set; } = default!;
    public ICollection<ProductEntry> ProductEntries { get; set; }
}