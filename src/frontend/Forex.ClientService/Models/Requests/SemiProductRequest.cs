namespace Forex.ClientService.Models.Requests;

public sealed record SemiProductRequest
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public long UnitMeasureId { get; set; }
    public string? ImagePath { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
}