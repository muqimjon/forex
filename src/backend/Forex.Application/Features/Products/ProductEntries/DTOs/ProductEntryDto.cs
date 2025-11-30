namespace Forex.Application.Features.Products.ProductEntries.DTOs;

using Forex.Application.Features.Currencies.DTOs;
using Forex.Application.Features.Products.ProductResidues.DTOs;
using Forex.Application.Features.Products.ProductTypes.DTOs;
using Forex.Application.Features.Shops.DTOs;
using Forex.Domain.Entities;
using Forex.Domain.Enums;

public sealed record ProductEntryDto
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public int BundleItemCount { get; set; }
    public decimal CostPrice { get; set; }
    public decimal PreparationCostPerUnit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public ProductionOrigin ProductionOrigin { get; set; }

    public long CurrencyId { get; set; }
    public CurrencyDto Currency { get; set; } = default!;

    public long ProductTypeId { get; set; }
    public ProductTypeForProductEntryDto ProductType { get; set; } = default!;

    public long ShopId { get; set; }
    public ShopForProductEntryDto Shop { get; set; } = default!;

    public long ProductResidueId { get; set; }
    public ProductResidueForProductEntryDto ProductResidue { get; set; } = default!;
}