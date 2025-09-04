namespace Forex.Domain.Entities;

using Forex.Domain.Commons;
using Forex.Domain.Enums;

public class Transaction : Auditable
{
    public int CashId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public int CurrencyId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public bool IsIncome { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }

    public Cash Cash { get; set; } = default!;
    public User User { get; set; } = default!;
    public Currency Currency { get; set; } = default!;
}