namespace Forex.ClientService.Models.Responses;

public sealed record SaleItemResponse
{
    public long Id { get; set; }
    public uint BundleCount { get; set; }   // 1 ta razmer (6 ta ayoq kiyim) razmerlar soni
    public uint BundleItemCount { get; set; }
    public uint TotalCount { get; set; }    // (1 ta oyoq kiyim) 1 qatorda jami 50 ta ayoq kiyim sotildi
    public decimal CostPrice { get; set; }    // 1 ta savdoning bir qatorining tannarxi
    public decimal Benifit { get; set; } // 1 ta savdoning bir qatorining foydasi
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }   // 1 ta savdoning bir qatorining summasi

    public long SaleId { get; set; }
    public SaleResponse Sale { get; set; } = default!;

    public long ProductTypeId { get; set; }   // 24-29, 30-35, 36-41 razmeri idsi
    public ProductTypeResponse ProductType { get; set; } = default!;
}