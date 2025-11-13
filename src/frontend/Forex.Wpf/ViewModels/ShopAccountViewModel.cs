namespace Forex.Wpf.ViewModels;

public sealed record ShopAccountViewModel
{
    public long Id { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }

    public long ShopId { get; set; }

    public long CurrencyId { get; set; }
    public CurrencyViewModel Currency { get; set; } = default!;
}
