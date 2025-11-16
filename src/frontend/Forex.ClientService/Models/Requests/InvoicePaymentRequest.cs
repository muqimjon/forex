namespace Forex.ClientService.Models.Requests;

using Forex.ClientService.Enums;

public class InvoicePaymentRequest
{
    public long UserId { get; set; }
    public long CurrencyId { get; set; }
    public PaymentTarget Target { get; set; }
    public decimal ExchangeRate { get; set; }
}
