namespace Forex.Application.Features.ShopCashes.DTOs;

using Forex.Domain.Entities.Payments;
using Forex.Domain.Entities.Shops;

public class ShopCashDto
{
    public long Id { get; set; }
    public long ShopId { get; set; }
    public long CurrencyId { get; set; }
    public decimal Balance { get; set; }

    public Shop Shop { get; set; } = default!;
    public Currency Currency { get; set; } = default!;
}