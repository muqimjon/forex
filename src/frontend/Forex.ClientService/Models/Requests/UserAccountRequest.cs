namespace Forex.ClientService.Models.Requests;

public sealed record UserAccountRequest
{
    public long Id { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
    public decimal Balance { get; set; }

    public string? Description { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }

    public long UserId { get; set; }

    public long CurrencyId { get; set; }
    public CurrencyRequest Currency { get; set; } = default!;
}