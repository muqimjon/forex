namespace Forex.Application.Features.Sales.SaleItems.DTOs;

using Forex.Application.Features.Sales.DTOs;

public sealed record SaleItemForProductTypeDto
{
    public long Id { get; set; }
    public int PerTypeCount { get; set; }   // 1 ta razmer (6 ta ayoq kiyim) razmerlar soni
    public int Count { get; set; }    // (1 ta oyoq kiyim) 1 qatorda jami 50 ta ayoq kiyim sotildi
    public decimal CostPrice { get; set; }    // 1 ta savdoning bir qatorining tannarxi
    public decimal Benifit { get; set; } // 1 ta savdoning bir qatorining foydasi
    public decimal Amount { get; set; }   // 1 ta savdoning bir qatorining summasi

    public long SaleId { get; set; }
    public SaleDto Sale { get; set; } = default!;

    public long ProductTypeId { get; set; }   // 24-29, 30-35, 36-41 razmeri idsi
}