namespace Forex.ClientService.Models.Responses;

using Forex.ClientService.Enums;

public sealed record TransactionResponse
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal Discount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public bool IsIncome { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }

    public long OperationRecordId { get; set; }
    public OperationRecordResponse OperationRecord { get; set; } = default!;

    public long? ShopId { get; set; }
    public ShopResponse Shop { get; set; } = default!;

    public long UserId { get; set; }
    public UserResponse User { get; set; } = default!;

    public long CurrencyId { get; set; }
    public CurrencyResponse Currency { get; set; } = default!;
}