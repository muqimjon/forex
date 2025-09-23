namespace Forex.Domain.Entities.Manufactories;

using Forex.Domain.Commons;

public class Invoice : Auditable
{
    public DateTime EntryDate { get; set; }
    public decimal CostPrice { get; set; } 
    public decimal CostDelivery { get; set; }
    public decimal? TransferFee { get; set; }
    public int? Containers { get; set; }
    public bool ViaMiddleman { get; set; }

    public ICollection<SemiProduct> SemiProducts { get; set; } = default!;
}