namespace Forex.Application.Features.Products.ProductResidues.DTOs;

using Forex.Application.Features.Products.ProductEntries.DTOs;
using Forex.Application.Features.Products.ProductTypes.DTOs;
using Forex.Application.Features.Shops.DTOs;

public class ProductResidueDto
{
    public long Id { get; set; }
    public int Count { get; set; }  // 24-29,30-35 yoki 36-41 razmerlarda nechtadan borligi

    public long ProductTypeId { get; set; }
    public ProductTypeDto ProductType { get; set; } = default!;

    public long ShopId { get; set; }
    public ShopDto Shop { get; set; } = default!;

    public ICollection<ProductEntryDto> ProductEntries { get; set; } = default!;
}