namespace Forex.Domain.Entities;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Products;

public class Shop : Auditable
{
    public string Name { get; set; } = string.Empty;
    public string? NormalizedName { get; set; } = string.Empty;

    public ICollection<ShopAccount> ShopAccounts { get; set; } = default!;
    public ICollection<ProductEntry> ProductEntries { get; set; } = default!;
    public ICollection<ProductResidue> ProductResidues { get; set; } = default!;
    public ICollection<Transaction> Transactions { get; set; } = default!;
}