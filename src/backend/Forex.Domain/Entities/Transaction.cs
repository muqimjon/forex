namespace Forex.Domain.Entities;

using Forex.Domain.Commons;
using Forex.Domain.Enums;

public class Transaction : Auditable
{
    public DateTime Date { get; set; }
    public bool IsIncome { get; set; }
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal Discount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Description { get; set; }

    public long ShopId { get; set; }
    public Shop Shop { get; set; } = default!;

    public long UserId { get; set; }
    public User User { get; set; } = default!;

    public long CurrencyId { get; set; }
    public Currency Currency { get; set; } = default!;

    public long OperationRecordId { get; set; }
    public OperationRecord OperationRecord { get; set; } = default!;
}
