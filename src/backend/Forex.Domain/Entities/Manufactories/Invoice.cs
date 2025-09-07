namespace Forex.Domain.Entities.Manufactories;

using Forex.Domain.Commons;

public class Invoice : Auditable
{
    public DateTime EntryDate { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public decimal TransferFee { get; set; }

    public ICollection<ContainerEntry> ContainerEntries { get; set; } = default!;
    public ICollection<SemiProductEntry> SemiProductEntries { get; set; } = default!;
}