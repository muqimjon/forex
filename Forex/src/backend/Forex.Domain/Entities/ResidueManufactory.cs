namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class ResidueManufactory : Auditable
{
    public long SemiProductId { get; set; }
    public long ManufactoryId { get; set; }
    public decimal Quantity { get; set; }

    public SemiProduct SemiProduct { get; set; } = default!;
    public Manufactory Manufactory { get; set; } = default!;
}