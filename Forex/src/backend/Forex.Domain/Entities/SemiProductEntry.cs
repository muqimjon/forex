namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class SemiProductEntry : Auditable
{
    public long SemiProductId { get; set; }
    public long InvoceId { get; set; }
    public long ManufactoryId { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public decimal TransferFee { get; set; }

    public SemiProduct SemiProduct { get; set; } = default!;
    public Invoice Invoce { get; set; } = default!;
    public Manufactory Manufactory { get; set; } = default!;
}