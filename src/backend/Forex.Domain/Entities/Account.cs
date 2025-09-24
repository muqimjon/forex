namespace Forex.Domain.Entities;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Payments;
using Forex.Domain.Entities.Shops;
using Forex.Domain.Entities.Users;

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
}

public class ShopCashAccount : Account
{
    public long ShopCashId { get; set; }
    public Shop Shop { get; set; } = default!;
}