namespace Forex.Application.Features.Users.DTOs;

public record AccountDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long CurrencyId { get; set; }
    public string CurrencyName { get; set; } = string.Empty;

    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }
}
