namespace Forex.ClientService.Models.Requests;

public sealed record InProcessRequest
{
    public long Id { get; set; }
    public decimal Quantity { get; set; }
    public long SemiProductId { get; set; }
}