namespace Forex.Application.Features.Products.Products.DTOs;

using Forex.Application.Features.Products.ProductTypes.DTOs;
using Forex.Domain.Entities;

public sealed record ProductDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ImagePath { get; set; } = string.Empty;

    public long UnitMeasureId { get; set; }
    public UnitMeasure UnitMeasure { get; set; } = default!;


    public ICollection<ProductTypeForProductDto> ProductTypes { get; set; } = default!;
}
