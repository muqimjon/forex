namespace Forex.Domain.Entities;

using Forex.Domain.Commons;
using Forex.Domain.Entities.SemiProducts;

public class Invoice : Auditable
{
    public DateTime Date { get; set; }
    public string? Number { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public bool ViaConsolidator { get; set; }
    public int? ContainerCount { get; set; }
    public decimal? PricePerUnitContainer { get; set; }
    public decimal? ConsolidatorFee { get; set; }
    public decimal TotalAmount { get; set; }

    public long ManufactoryId { get; set; }
    public Manufactory Manufactory { get; set; } = default!;

    public ICollection<SemiProductEntry> SemiProductEntries { get; set; } = default!;
    public ICollection<InvoicePayment> Payments { get; set; } = default!;
}
