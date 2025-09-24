namespace Forex.Domain.Entities.Manufactories;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Users;

public class Invoice : Auditable
{
    public DateTime EntryDate { get; set; }
    public string? Number { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public long SupplierId { get; set; }
    public bool ViaMiddleman { get; set; }
    public long? SenderId { get; set; }
    public decimal? TransferFee { get; set; }
    public decimal TotalAmount { get; set; }
    public long ManufactoryId { get; set; }

    public Manufactory Manufactory { get; set; } = default!;
    public User Supplier { get; set; } = default!;
    public User Sender { get; set; } = default!;
    public ICollection<SemiProduct> SemiProducts { get; set; } = default!;
}