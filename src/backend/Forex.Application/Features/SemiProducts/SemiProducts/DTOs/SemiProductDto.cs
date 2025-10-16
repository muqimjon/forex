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
    public UnitMeasureDto UnitMeasuer { get; set; } = default!;

    public ICollection<ProductTypeItemDto>? ProductTypeItem { get; set; }
    public ICollection<SemiProductResidueDto> SemiProductResidues { get; set; } = default!;
    public ICollection<SemiProductEntryDto> SemiProductEntries { get; set; } = default!;
}
