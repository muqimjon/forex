namespace Forex.Application.Features.SemiProducts.SemiProducts.DTOs;

using Forex.Application.Features.Products.ProductTypeItems.DTOs;
using Forex.Application.Features.SemiProducts.SemiProductEntries.DTOs;
using Forex.Application.Features.SemiProducts.SemiProductResidues.DTOs;
using Forex.Application.Features.UnitMeasures.DTOs;

public sealed record SemiProductDto
{
    public long Id { get; init; }
    public string? Name { get; set; }
    public string? ImagePath { get; set; }

    public long UnitMeasureId { get; set; }
    public UnitMeasureDto UnitMeasure { get; set; } = default!;

    public ICollection<ProductTypeItemForSemiProductDto>? ProductTypeItem { get; set; }
    public ICollection<SemiProductResidueForSemiProdutDto> SemiProductResidues { get; set; } = default!;
    public ICollection<SemiProductEntryForSemiProductDto> SemiProductEntries { get; set; } = default!;
}
