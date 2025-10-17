namespace Forex.Domain.Entities.SemiProducts;

using Forex.Domain.Commons;
using Forex.Domain.Entities;

public class SemiProductResidue : Auditable
{
    public decimal Quantity { get; set; }

    public long ManufactoryId { get; set; }
    public Manufactory Manufactory { get; set; } = default!;

    public long SemiProductId { get; set; }
    public SemiProduct SemiProduct { get; set; } = default!;
}