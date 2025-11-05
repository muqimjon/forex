namespace Forex.Application.Features.UnitMeasures.Commands;

public sealed record UnitMeasureCommand
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public int Position { get; set; }
}