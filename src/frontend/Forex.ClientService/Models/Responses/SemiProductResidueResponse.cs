namespace Forex.ClientService.Models.Responses;

public sealed record SemiProductResidueResponse
{
    public long Id { get; set; }
    public long SemiProductId { get; set; }
    public long ManufactoryId { get; set; }
    public decimal Quantity { get; set; }
    public SemiProductResponse SemiProduct { get; set; } = default!;
}