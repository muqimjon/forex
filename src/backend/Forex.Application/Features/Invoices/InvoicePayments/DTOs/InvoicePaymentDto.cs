namespace Forex.Application.Features.Invoices.InvoicePayments.DTOs;

using Forex.Application.Features.Currencies.DTOs;
using Forex.Application.Features.Invoices.Invoices.DTOs;
using Forex.Application.Features.Users.DTOs;
using Forex.Domain.Enums;

public class InvoicePaymentDto
{
    public long Id { get; set; }
    public long InvoiceId { get; set; }
    public InvoiceForInvoicePaymentDto Invoice { get; set; } = default!;

    public long UserId { get; set; }
    public UserDto User { get; set; } = default!;

    public long CurrencyId { get; set; }
    public CurrencyDto Currency { get; set; } = default!;

    public PaymentTarget Target { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal Amount { get; set; }
}
