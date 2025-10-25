namespace Forex.Domain.Entities.Sales;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Products;

public class SaleItem : Auditable
{
    public int BundleCount { get; set; }   // 1 ta razmer (6 ta ayoq kiyim) razmerlar soni
    public int BundleItemCount { get; set; }
    public int TotalCount { get; set; }    // (1 ta oyoq kiyim) 1 qatorda jami 50 ta ayoq kiyim sotildi
    public decimal CostPrice { get; set; }    // 1 ta savdoning bir qatorining tannarxi
    public decimal Benifit { get; set; } // 1 ta savdoning bir qatorining foydasi
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }   // 1 ta savdoning bir qatorining summasi

    public long SaleId { get; set; }
    public Sale Sale { get; set; } = default!;

    public long ProductTypeId { get; set; }   // 24-29, 30-35, 36-41 razmeri idsi
    public ProductType ProductType { get; set; } = default!;
}