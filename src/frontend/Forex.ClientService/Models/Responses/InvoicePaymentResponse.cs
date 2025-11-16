namespace Forex.ClientService.Models.Responses;

using Forex.ClientService.Enums;

public class InvoicePaymentResponse
{
    public long Id { get; set; }
    public long InvoiceId { get; set; }
    public InvoiceResponse Invoice { get; set; } = default!;

    public long UserId { get; set; }
    public UserResponse User { get; set; } = default!;

    public long CurrencyId { get; set; }
    public CurrencyResponse Currency { get; set; } = default!;

    public PaymentTarget Target { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal Amount { get; set; }
}