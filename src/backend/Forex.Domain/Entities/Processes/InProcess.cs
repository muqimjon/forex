namespace Forex.Domain.Entities.Processes;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Products;

public class InProcess : Auditable
{
    public decimal Quantity { get; set; }

    public long ProductTypeId { get; set; }
    public ProductType ProductType { get; set; } = default!;

    public ICollection<EntryToProcess> EntryToProcesses { get; set; } = default!;
}
