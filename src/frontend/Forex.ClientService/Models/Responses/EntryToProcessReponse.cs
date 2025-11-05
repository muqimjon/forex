namespace Forex.ClientService.Models.Responses;

public sealed record EntryToProcessReponse
{
    public long Id { get; set; }
    public decimal Quantity { get; set; }

    public long SemiProductId { get; set; }
    public SemiProductResponse SemiProduct { get; set; } = default!;
}