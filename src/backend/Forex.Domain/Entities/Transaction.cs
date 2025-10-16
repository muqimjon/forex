namespace Forex.Domain.Entities;

using Forex.Domain.Commons;
using Forex.Domain.Enums;

public class Transaction : Auditable
{
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public bool IsIncome { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }

    public long ShopId { get; set; }
    public Shop Shop { get; set; } = default!;

    public long UserId { get; set; }
    public User User { get; set; } = default!;

    public long CurrencyId { get; set; }
    public Currency Currency { get; set; } = default!;
}