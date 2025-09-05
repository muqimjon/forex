namespace Forex.Application.Features.Cashes.DTOs;

using Forex.Domain.Entities;

public class CashDto
{
    public long Id { get; set; }
    public long ShopId { get; set; }
    public long CurrencyId { get; set; }
    public decimal Balance { get; set; }

    public Shop Shop { get; set; } = default!;
    public Currency Currency { get; set; } = default!;
}