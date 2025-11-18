namespace Forex.Application.Features.Products.Products.DTOs;

using Forex.Application.Features.UnitMeasures.DTOs;

public sealed record ProductForProductTypeDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ImagePath { get; set; } = string.Empty;

    public long UnitMeasureId { get; set; }
    public UnitMeasureDto UnitMeasure { get; set; } = default!;
}