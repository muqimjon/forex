namespace Forex.Domain.Entities.Products;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Processes;

public class ProductType : Auditable
{
    public string Type { get; set; } = string.Empty;
    public int BundleItemCount { get; set; }
    public decimal UnitPrice { get; set; }

    public long ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public long CurrencyId { get; set; }
    public Currency Currency { get; set; } = default!;

    public ProductResidue? ProductResidue { get; set; } = default!;
    public InProcess? InProcess { get; set; } = default!;

    public ICollection<ProductTypeItem> ProductTypeItems { get; set; } = default!;
    public ICollection<ProductEntry> ProductEntries { get; set; } = default!;
    public ICollection<EntryToProcess> EntryToProcesses { get; set; } = default!;
}