namespace Forex.Domain.Entities.Products;

using Forex.Domain.Commons;
using Forex.Domain.Entities;

public class ProductResidue : Auditable
{
    public int Count { get; set; }

    public long ProductTypeId { get; set; }
    public ProductType ProductType { get; set; } = default!;

    public long ShopId { get; set; }
    public Shop Shop { get; set; } = default!;

    public ICollection<ProductEntry> ProductEntries { get; set; } = default!;
}