namespace Forex.Application.Features.Products.ProductEntries.DTOs;

using Forex.Application.Features.Products.ProductTypes.DTOs;
using Forex.Application.Features.Shops.DTOs;
using Forex.Domain.Enums;

public sealed record ProductEntryDto
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public int BundleItemCount { get; set; }
    public decimal CostPrice { get; set; }     // tannarxi
    public decimal PreparationCostPerUnit { get; set; }  // tayyorlashga ketgan xarajat summasi
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public ProductionOrigin ProductionOrigin { get; set; }

    public long ProductTypeId { get; set; }  // 
    public ProductTypeForProductEntryDto ProductType { get; set; } = default!;  // razmeri 24-29, 30-35, 36-41

    public long ShopId { get; set; }
    public ShopForProductEntryDto Shop { get; set; } = default!;
}
