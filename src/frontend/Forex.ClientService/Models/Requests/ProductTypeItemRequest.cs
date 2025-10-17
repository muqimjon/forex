namespace Forex.ClientService.Models.Requests;

public sealed record ProductTypeItemRequest
{
    public decimal Quantity { get; set; }
    public SemiProductRequest SemiProduct { get; set; } = default!;
}