namespace Forex.Domain.Entities;

using Forex.Domain.Commons;
using Forex.Domain.Entities.SemiProducts;

public class Invoice : Auditable
{
    public DateTime Date { get; set; }
    public string? Number { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public bool ViaMiddleman { get; set; }
    public int ContainerCount { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal? TransferFee { get; set; }
    public decimal TotalAmount { get; set; }

    public long CurrencyId { get; set; }
    public Currency Currency { get; set; } = default!;

    public long ManufactoryId { get; set; }
    public Manufactory Manufactory { get; set; } = default!;

    public long SupplierId { get; set; }
    public User Supplier { get; set; } = default!;

    public long? SenderId { get; set; }
    public User Sender { get; set; } = default!;

    public ICollection<SemiProductEntry> SemiProductEntries { get; set; } = default!;
}