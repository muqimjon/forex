namespace Forex.ClientService.Models.Requests;

public sealed record UserAccount
{
    public long CurrencyId { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
}
