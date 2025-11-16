namespace Forex.Application.Features.Accounts.Commands;

public class CreateUserAccountCommand
{
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }

    public long CurrencyId { get; set; }
}