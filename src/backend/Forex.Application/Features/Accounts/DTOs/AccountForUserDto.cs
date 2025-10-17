namespace Forex.Application.Features.Accounts.DTOs;

using Forex.Application.Features.Currencies.DTOs;

public class AccountForUserDto
{
    public long Id { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }

    public long UserId { get; set; }

    public long CurrencyId { get; set; }
    public CurrencyDto Currency { get; set; } = default!;
}