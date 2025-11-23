namespace Forex.Application.Features.Products.ProductEntries.DTOs;

using Forex.Application.Features.Products.ProductTypes.DTOs;
using Forex.Domain.Enums;

public sealed record ProductEntryForShopDto
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

    public long ProductTypeId { get; set; }
    public ProductTypeDto ProductType { get; set; } = default!;

    public long ShopId { get; set; }
}