namespace Forex.Application.Features.Invoices.InvoicePayments.Commands;

using Forex.Domain.Enums;

public class InvoicePaymentCommand
{
    public long UserId { get; set; }
    public long CurrencyId { get; set; }
    public PaymentTarget Target { get; set; }
    public decimal ExchangeRate { get; set; }
}
