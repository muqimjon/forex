namespace Forex.Domain.Entities.Manufactories;

using Forex.Domain.Commons;

public class SemiProductResidue : Auditable
{
    public long SemiProductId { get; set; }
    public long ManufactoryId { get; set; }
    public decimal Quantity { get; set; }

    public SemiProduct SemiProduct { get; set; } = default!;
    public Manufactory Manufactory { get; set; } = default!;
}