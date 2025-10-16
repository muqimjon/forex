namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class Currency : Auditable
{
    public string Name { get; set; } = string.Empty;
    public string? NormalizedName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Transaction> Transactions { get; set; } = default!;
    public ICollection<Invoice> Invoices { get; set; } = default!;
    public ICollection<UserAccount> UserAccounts { get; set; } = default!;
    public ICollection<ShopAccount> ShopCashAccounts { get; set; } = default!;
}