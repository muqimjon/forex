namespace Forex.Domain.Entities.SemiProducts;

using Forex.Domain.Commons;
using Forex.Domain.Entities;

public class SemiProductEntry : Auditable
{
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public decimal ConsolidatorFee { get; set; }

    public long InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = default!;

    public long SemiProductId { get; set; }
    public SemiProduct SemiProduct { get; set; } = default!;

    public long ManufactoryId { get; set; }
    public Manufactory Manufactory { get; set; } = default!;
}