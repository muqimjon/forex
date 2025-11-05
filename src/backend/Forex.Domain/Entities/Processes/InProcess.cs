namespace Forex.Domain.Entities.Processes;

using Forex.Domain.Commons;
using Forex.Domain.Entities.SemiProducts;

public class InProcess : Auditable
{
    public decimal Quantity { get; set; }

    public long SemiProductId { get; set; }
    public SemiProduct SemiProduct { get; set; } = default!;
}
