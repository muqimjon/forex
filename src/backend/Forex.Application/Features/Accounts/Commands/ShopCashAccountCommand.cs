namespace Forex.Application.Features.Accounts.Commands;


public sealed record CreateShopAccountCommand
{
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }

    public long ShopCashId { get; set; }
    public long CurrencyId { get; set; }
}
