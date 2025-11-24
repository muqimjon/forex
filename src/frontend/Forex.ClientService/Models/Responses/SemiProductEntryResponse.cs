namespace Forex.ClientService.Models.Responses;

public sealed record SemiProductEntryResponse
{
    public long Id { get; init; }
    public long SemiProductId { get; init; }
    public decimal Amount { get; init; }
    public decimal PricePerUnit { get; init; }
    public decimal TotalPrice { get; init; }
    public DateTime EntryDate { get; init; }
}