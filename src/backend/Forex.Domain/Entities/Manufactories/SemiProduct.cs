namespace Forex.Domain.Entities.Manufactories;

using Forex.Domain.Commons;

public class SemiProduct : Auditable
{
    public string? Name { get; set; }
    public string? NormalizedName { get; set; } = string.Empty;
    public int Code { get; set; }
    public string Measure { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }

    public ICollection<SemiProductResidue> SemiProductResidues { get; set; } = default!;
    public ICollection<SemiProductEntry> SemiProductEntries { get; set; } = default!;
}