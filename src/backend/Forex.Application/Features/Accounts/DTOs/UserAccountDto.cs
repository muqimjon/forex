namespace Forex.Application.Features.Accounts.DTOs;

using Forex.Application.Features.Currencies.DTOs;

public sealed record UserAccountDto
{
    public long Id { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }

    public long UserId { get; set; }
    //public UserForAccountDto User { get; set; } = default!;

    public long CurrencyId { get; set; }
    public CurrencyDto Currency { get; set; } = default!;
}