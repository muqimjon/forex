namespace Forex.Application.Features.Products.ProductResidues.DTOs;

using Forex.Application.Features.Products.ProductEntries.DTOs;
using Forex.Application.Features.Products.ProductTypes.DTOs;
using Forex.Application.Features.Shops.DTOs;

public sealed record ProductResidueDto
{
    public long Id { get; set; }
    public int Count { get; set; }  // 24-29,30-35 yoki 36-41 razmerlarda nechtadan borligi

    public long ProductTypeId { get; set; }
    public ProductTypeForProductResidueDto ProductType { get; set; } = default!;

    public long ShopId { get; set; }
    public ShopForProductResidueDto Shop { get; set; } = default!;

    public ICollection<ProductEntryForProductResidueDto> ProductEntries { get; set; } = default!;
}
