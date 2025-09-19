namespace Forex.ClientService.Models.Users;

public sealed record Account
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public decimal BeginSumm { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }
}