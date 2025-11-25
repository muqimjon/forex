namespace Forex.Application.Features.Transactions.DTOs;

using Forex.Application.Features.Currencies.DTOs;
using Forex.Application.Features.OperationRecords.DTOs;
using Forex.Application.Features.Users.DTOs;
using Forex.Domain.Enums;

public sealed record TransactionForShopDto
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
    public OperationRecordForTransactionDto OperationRecord { get; set; } = default!;

    public long? ShopId { get; set; }

    public long UserId { get; set; }
    public UserForTransactionDto User { get; set; } = default!;

    public long CurrencyId { get; set; }
    public CurrencyDto Currency { get; set; } = default!;
}