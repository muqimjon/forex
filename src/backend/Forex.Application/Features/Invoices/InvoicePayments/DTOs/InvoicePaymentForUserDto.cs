namespace Forex.Application.Features.Invoices.InvoicePayments.DTOs;

using Forex.Domain.Entities;
using Forex.Domain.Enums;

public class InvoicePaymentForUserDto
{
    public long Id { get; set; }
    public long InvoiceId { get; set; }

    public long UserId { get; set; }

    public long CurrencyId { get; set; }
    public Currency Currency { get; set; } = default!;

    public PaymentTarget Target { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal Amount { get; set; }
}
