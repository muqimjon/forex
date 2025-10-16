namespace Forex.Application.Features.Accounts.DTOs;

using Forex.Application.Features.Shops.DTOs;
using Forex.Application.Features.Currencies.DTOs;

public class ShopAccountDto
{
    public long Id { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }

    public long ShopId { get; set; }
    public ShopDto Shop { get; set; } = default!;

    public long CurrencyId { get; set; }
    public CurrencyDto Currency { get; set; } = default!;
}