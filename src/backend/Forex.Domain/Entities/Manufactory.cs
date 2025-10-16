namespace Forex.Domain.Entities;

using Forex.Domain.Entities.SemiProducts;
using Forex.Domain.Commons;

public class Manufactory : Auditable
{
    public string Name { get; set; } = string.Empty;
    public string? NormalizedName { get; set; } = string.Empty;

    public ICollection<SemiProductResidue> SemiProductResidues { get; set; } = default!;
    public ICollection<SemiProductEntry> SemiProductEntries { get; set; } = default!;
    public ICollection<Invoice> Invoices { get; set; } = default!;
}