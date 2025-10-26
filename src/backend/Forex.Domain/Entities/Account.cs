namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class Account : Auditable
{
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }
    public long CurrencyId { get; set; }
    public Currency Currency { get; set; } = default!;
}

public class UserAccount : Account
{
    public long UserId { get; set; }
    public User User { get; set; } = default!;
    public string? Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
}

public class ShopAccount : Account
{
    public long ShopId { get; set; }
    public Shop Shop { get; set; } = default!;
}