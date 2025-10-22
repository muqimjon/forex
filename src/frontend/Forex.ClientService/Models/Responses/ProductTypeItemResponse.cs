namespace Forex.ClientService.Models.Responses;

public record ProductTypeItemResponse
{
    public long Id { get; set; }
    public decimal Quantity { get; set; }   // miqdori

    public long ProductTypeId { get; set; }
    public ProductTypeResponse ProductType { get; set; } = default!;

    public long SemiProductId { get; set; }
    public SemiProductResponse SemiProduct { get; set; } = default!;
}