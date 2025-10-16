namespace Forex.ClientService.Models.Responses;

public sealed record UserAccountResponse
{
    public long Id { get; set; }

    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }

    public long UserId { get; set; }
    public UserResponse User { get; set; } = default!;

    public long CurrencyId { get; set; }
    public CurrencyResponse Currency { get; set; } = default!;
}