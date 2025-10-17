namespace Forex.Application.Features.SemiProducts.SemiProducts.Commands;

public sealed record SemiProductCommand
{
    public string? Name { get; set; }
    public long UnitMeasureId { get; set; }
    public int Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public string? ImagePath { get; set; } = string.Empty;
}