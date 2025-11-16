namespace Forex.Domain.Entities;

using Forex.Domain.Commons;
using Forex.Domain.Enums;

public class InvoicePayment : Auditable
{
    public long InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = default!;

    public long UserId { get; set; }
    public User User { get; set; } = default!;

    public long CurrencyId { get; set; }
    public Currency Currency { get; set; } = default!;

    public PaymentTarget Target { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal Amount { get; set; }
}
