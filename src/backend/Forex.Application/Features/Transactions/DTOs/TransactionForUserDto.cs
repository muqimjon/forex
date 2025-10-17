namespace Forex.Application.Features.Transactions.DTOs;

using Forex.Application.Features.Currencies.DTOs;
using Forex.Application.Features.Shops.DTOs;
using Forex.Domain.Enums;

public sealed record TransactionForUserDto
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public bool IsIncome { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }

    public long ShopId { get; set; }
    public ShopForTransactionDto Shop { get; set; } = default!;

    public long UserId { get; set; }

    public long CurrencyId { get; set; }
    public CurrencyDto Currency { get; set; } = default!;
}