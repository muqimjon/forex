namespace Forex.ClientService.Models.Users;

public sealed record CurrencyBalanceRequest
{
    public long CurrencyId { get; set; }
    public decimal Balance { get; set; }
    public decimal Discount { get; set; }
}