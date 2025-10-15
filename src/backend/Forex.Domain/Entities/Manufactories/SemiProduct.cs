namespace Forex.Domain.Entities.Manufactories;

using Forex.Domain.Commons;
using Forex.Domain.Entities;

public class SemiProduct : Auditable
{
    public string? Name { get; set; }
    public string? SearchName { get; set; } = string.Empty;
    public int Code { get; set; }
    public long MeasureId { get; set; }
    public string? PhotoPath { get; set; }

    public UnitMeasure UnitMeasuer { get; set; } = default!;
    public ICollection<SemiProductResidue> SemiProductResidues { get; set; } = default!;
    public ICollection<SemiProductEntry> SemiProductEntries { get; set; } = default!;
}
