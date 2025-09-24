namespace Forex.Domain.Entities.Shops;

using Forex.Domain.Commons;
using Forex.Domain.Entities;
using System.Transactions;

public class Shop : Auditable
{
    public string Name { get; set; } = string.Empty;
    public string? SearchName { get; set; } = string.Empty;

    public ICollection<ShopCashAccount> ShopCashes { get; set; } = default!;
    public ICollection<ProductEntry> ProductEntries { get; set; } = default!;
    public ICollection<ProductResidue> ProductResidues { get; set; } = default!;
    public ICollection<Transaction> Transactions { get; set; } = default!;
}