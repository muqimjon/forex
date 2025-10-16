namespace Forex.Application.Features.Products.ProductTypes.DTOs;

using Forex.Application.Features.Products.ProductEntries.DTOs;
using Forex.Application.Features.Products.ProductResidues.DTOs;
using Forex.Application.Features.Products.Products.DTOs;
using Forex.Application.Features.Products.ProductTypeItems.DTOs;

public sealed record ProductTypeDto
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;    //24-29, 30-35 , 36-41 razmeri
    public int Count { get; set; }     // 24-29 to'plamda nechtadan mahsulot borligi

    public long ProductId { get; set; }
    public ProductForProductTypeDto Product { get; set; } = default!;

    public ProductResidueForProductTypeDto ProductResidue { get; set; } = default!;

    public ICollection<ProductTypeItemForProductTypeDto> Items { get; set; } = default!;
    public ICollection<ProductEntryForProductTypeDto> Entries { get; set; } = default!;
}
