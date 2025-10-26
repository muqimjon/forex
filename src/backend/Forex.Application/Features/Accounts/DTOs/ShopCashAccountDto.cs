namespace Forex.Application.Features.Accounts.DTOs;

using Forex.Application.Features.Currencies.DTOs;
using Forex.Application.Features.Shops.DTOs;

public sealed record ShopAccountDto
{
    public long Id { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }

    public long ShopId { get; set; }
    public ShopForAccountDto Shop { get; set; } = default!;

    public long CurrencyId { get; set; }
    public CurrencyDto Currency { get; set; } = default!;
}