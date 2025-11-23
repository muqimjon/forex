namespace Forex.Domain.Entities.Products;

using Forex.Domain.Commons;
using Forex.Domain.Enums;

public class Product : Auditable
{
    public string Name { get; set; } = string.Empty;
    public string? NormalizedName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ImagePath { get; set; } = string.Empty;
    public ProductionOrigin ProductionOrigin { get; set; }

    public long UnitMeasureId { get; set; }
    public UnitMeasure UnitMeasure { get; set; } = default!;

    public ICollection<ProductType> ProductTypes { get; set; } = default!;
}
