namespace Forex.Domain.Entities.Processes;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Products;

public class EntryToProcess : Auditable
{
    public int Count { get; set; }

    public long ProductTypeId { get; set; }
    public ProductType ProductType { get; set; } = default!;

    public long InProcessId { get; set; }
    public InProcess InProcess { get; set; } = default!;
}