namespace Forex.ClientService.Models.Users;

public sealed record Account
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long CurrencyId { get; set; }

    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }
}