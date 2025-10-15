namespace Forex.ClientService.Models.Requests;

public sealed record CurrencyBalanceRequest
{
    public long CurrencyId { get; set; }
    public decimal Balance { get; set; }
    public decimal Discount { get; set; }
}
