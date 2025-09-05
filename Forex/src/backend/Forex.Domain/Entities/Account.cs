namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class Account : Auditable
{
    public long UserId { get; set; }
    public decimal BeginSumm { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }

    public User User { get; set; } = default!;
}
