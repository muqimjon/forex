namespace Forex.Application.Features.Accounts.DTOs;

public class CreateUserAccountCommand
{
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }

    public long CurrencyId { get; set; }
}

public class UpdateUserAccountCommand
{
    public long Id { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }

    public long UserId { get; set; }
    public long CurrencyId { get; set; }
}