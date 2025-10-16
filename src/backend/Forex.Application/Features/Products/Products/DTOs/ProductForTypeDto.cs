namespace Forex.Application.Features.Products.Products.DTOs;

using Forex.Application.Features.UnitMeasures.DTOs;

public sealed record ProductForTypeDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Code { get; set; }  // mahsulot kodi 101 misol uchun
    public string? ImagePath { get; set; } = string.Empty;

    public long MeasureId { get; set; }
    public UnitMeasureDto Measure { get; set; } = default!;
}