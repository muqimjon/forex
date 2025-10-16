namespace Forex.Application.Features.Accounts.Commands;

public class UpdateUserAccountCommand
{
    public long Id { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }

    public long UserId { get; set; }
    public long CurrencyId { get; set; }
}