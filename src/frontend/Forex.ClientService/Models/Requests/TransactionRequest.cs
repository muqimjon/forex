namespace Forex.ClientService.Models.Requests;

using Forex.ClientService.Enums;

public sealed record TransactionRequest
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal Discount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public bool IsIncome { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }

    public long UserId { get; set; }
    public long CurrencyId { get; set; }
}