namespace Forex.Application.Features.Transactions.DTOs;

using Forex.Application.Features.Currencies.DTOs;
using Forex.Application.Features.Shops.DTOs;
using Forex.Application.Features.Users.DTOs;
using Forex.Domain.Enums;

public class TransactionDto
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public bool IsIncome { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }

    public long ShopId { get; set; }
    public ShopDto Shop { get; set; } = default!;

    public long UserId { get; set; }
    public UserDto User { get; set; } = default!;

    public long CurrencyId { get; set; }
    public CurrencyDto Currency { get; set; } = default!;
}