namespace Forex.Application.Features.Accounts.Commands;

public sealed record UpdateShopAccountCommand
{
    public long Id { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }

    public long ShopCashId { get; set; }
    public long CurrencyId { get; set; }
}