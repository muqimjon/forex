namespace Forex.Domain.Entities.Sales;

using Forex.Domain.Commons;
using Forex.Domain.Entities.Shops;

public class SaleItem : Auditable
{
    public long SaleId { get; set; }
    public long ProductTypeId { get; set; }   // 24-29, 30-35, 36-41 razmeri idsi
    public int TypeCount { get; set; }   // 1 ta razmer (6 ta ayoq kiyim) razmerlar soni
    public int Count { get; set; }    // (1 ta ayoq kiyim) 1 qatorda jami 50 ta ayoq kiyim sotildi
    public decimal CostPrice { get; set; }    // 1 ta savdoning bir qatorining tannarxi
    public decimal Benifit { get; set; } // 1 ta savdoning bir qatorining foydasi
    public decimal Amount { get; set; }   // 1 ta savdoning bir qatorining summasi

    public Sale Sale { get; set; } = default!;
    public ProductType ProductType { get; set; } = default!;
}