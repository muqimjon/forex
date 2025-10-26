namespace Forex.ClientService.Models.Responses;

public sealed record ShopAccountResponse
{
    public long Id { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }

    public long ShopId { get; set; }
    public ShopResponse Shop { get; set; } = default!;

    public long CurrencyId { get; set; }
    public CurrencyResponse Currency { get; set; } = default!;
}