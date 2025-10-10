namespace Forex.Domain.Entities.Shops;

using Forex.Domain.Commons;

public class ProductResidue : Auditable
{
    public long ShopId { get; set; }
    public long ProductTypeId { get; set; }
    public int TypeCount { get; set; }  // 24-29,30-35 yoki 36-41 razmerlarda nechtadan borligi

    public ProductType ProductType { get; set; } = default!;
    public Shop Shop { get; set; } = default!;
    public ICollection<ProductEntry> ProductEntries { get; set; }
}