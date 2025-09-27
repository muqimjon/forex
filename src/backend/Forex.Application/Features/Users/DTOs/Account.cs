namespace Forex.Application.Features.Users.DTOs;

using Forex.Domain.Entities.Payments;

public record AccountDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long CurrencyId { get; set; }
    public Currency Currency { get; set; } = default!;

    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }


}
