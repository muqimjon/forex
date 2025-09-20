namespace Forex.Domain.Entities.Shops;

using Forex.Domain.Commons;
using System.Transactions;

public class Shop : Auditable
{
    public string Name { get; set; } = string.Empty;
    public string? NormalizedName { get; set; } = string.Empty;

    public ICollection<ShopCash> ShopCashes { get; set; } = default!;
    public ICollection<ProductEntry> ProductEntries { get; set; } = default!;
    public ICollection<ProductResidue> ProductResidues { get; set; } = default!;
    public ICollection<Transaction> Transactions { get; set; } = default!;
}