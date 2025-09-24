namespace Forex.Domain.Entities.Shops;

using Forex.Domain.Commons;

public class Product : Auditable
{
    public string Name { get; set; } = string.Empty;
    public string? SearchName { get; set; } = string.Empty;
    public int Code { get; set; }
    public long MeasureId { get; set; }
    public string? PhotoPath { get; set; } = string.Empty;

    public UnitMeasuer Measure { get; set; } = default!;
    public ICollection<ProductType> ProductTypes { get; set; } = default!;
}
