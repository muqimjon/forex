namespace Forex.Domain.Entities.Products;

using Forex.Domain.Commons;
using Forex.Domain.Entities;
using Forex.Domain.Enums;

public class ProductEntry : Auditable
{
    public int Count { get; set; }
    public DateTime Date { get; set; }
    public int BundleItemCount { get; set; }
    public decimal CostPrice { get; set; }
    public decimal PreparationCostPerUnit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public ProductionOrigin ProductionOrigin { get; set; }

    public long CurrencyId { get; set; }
    public Currency Currency { get; set; } = default!;

    public long ProductTypeId { get; set; }
    public ProductType ProductType { get; set; } = default!;

    public long ShopId { get; set; }
    public Shop Shop { get; set; } = default!;

    public long ProductResidueId { get; set; }
    public ProductResidue ProductResidue { get; set; } = default!;
}