namespace Forex.Domain.Entities;

using Forex.Domain.Commons;
using Forex.Domain.Enums;

public class Transaction : Auditable
{
    public long CashId { get; set; }
    public long UserId { get; set; }
    public decimal Amount { get; set; }
    public long CurrencyId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public bool IsIncome { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }

    public Cash Cash { get; set; } = default!;
    public User User { get; set; } = default!;
    public Currency Currency { get; set; } = default!;
}