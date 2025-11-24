namespace Forex.Domain.Entities.Products;

using Forex.Domain.Commons;
using Forex.Domain.Entities;
using Forex.Domain.Enums;

public class ProductEntry : Auditable
{
    public int Count { get; set; }
    public DateTime Date { get; set; }
    public int BundleItemCount { get; set; }
    public decimal CostPrice { get; set; }     // tannarxi
    public decimal PreparationCostPerUnit { get; set; }  // tayyorlashga ketgan xarajat summasi
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public ProductionOrigin ProductionOrigin { get; set; }

    public long ProductTypeId { get; set; }
    public ProductType ProductType { get; set; } = default!;  // razmeri 24-29, 30-35, 36-41

    public long ShopId { get; set; }
    public Shop Shop { get; set; } = default!;

    public long ProductResidueId { get; set; }
    public ProductResidue ProductResidue { get; set; } = default!;
}