namespace Forex.Domain.Entities.Manufactories;

using Forex.Domain.Commons;

public class Manufactory : Auditable
{
    public string Name { get; set; } = string.Empty;
    public string? SearchName { get; set; } = string.Empty;

    public ICollection<SemiProductResidue> SemiProductResidues { get; set; } = default!;
    public ICollection<SemiProductEntry> SemiProductEntries { get; set; } = default!;
}