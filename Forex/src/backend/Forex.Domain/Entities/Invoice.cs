namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class Invoice : Auditable
{
    public DateTime EntryDate { get; set; }
    public decimal CostPrice { get; set; }
    public decimal CostDelivery { get; set; }
    public decimal TransferFee { get; set; }

    public ICollection<ContainerEntry> ContainerEntries { get; set; } = new List<ContainerEntry>();
    public ICollection<SemiProductEntry> SameProductEntries { get; set; } = new List<SemiProductEntry>();
}