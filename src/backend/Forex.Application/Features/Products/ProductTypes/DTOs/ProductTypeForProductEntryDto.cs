namespace Forex.Application.Features.Products.ProductTypes.DTOs;

using Forex.Application.Features.Currencies.DTOs;
using Forex.Application.Features.Products.ProductResidues.DTOs;
using Forex.Application.Features.Products.Products.DTOs;

public sealed record ProductTypeForProductEntryDto
{
    public long Id { get; set; }
    public string Type { get; set; } = string.Empty;    //24-29, 30-35 , 36-41 razmeri
    public int BundleItemCount { get; set; }     // 24-29 to'plamda nechtadan mahsulot borligi
    public int PackItemCount { get; set; }
    public decimal UnitPrice { get; set; }

    public string? QopBarcode { get; set; }
    public string? PackBarcode { get; set; }

    public long CurrencyId { get; set; }
    public CurrencyDto Currency { get; set; } = default!;

    public long ProductId { get; set; }
    public ProductForProductTypeDto Product { get; set; } = default!;

    public ProductResidueForProductTypeDto ProductResidue { get; set; } = default!;
}