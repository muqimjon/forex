namespace Forex.Domain.Entities.Shops;

using Forex.Domain.Commons;

public class ProductType : Auditable
{
    public long ProductId { get; set; }
    public string Type { get; set; } = string.Empty;
    public long ProductResidueId { get; set; }

    public ProductResidue ProductResidue { get; set; } = default!;
    public ICollection<ProductTypeItem> Items { get; set; } = default!;
}