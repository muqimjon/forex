namespace Forex.Domain.Entities.SemiProducts;

using Forex.Domain.Commons;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Products;

public class SemiProduct : Auditable
{
    public string? Name { get; set; }
    public string? NormalizedName { get; set; } = string.Empty;
    public string? ImagePath { get; set; }

    public long UnitMeasureId { get; set; }
    public UnitMeasure UnitMeasuer { get; set; } = default!;

    public ICollection<ProductTypeItem>? ProductTypeItem { get; set; }
    public ICollection<SemiProductResidue> SemiProductResidues { get; set; } = default!;
    public ICollection<SemiProductEntry> SemiProductEntries { get; set; } = default!;
}
