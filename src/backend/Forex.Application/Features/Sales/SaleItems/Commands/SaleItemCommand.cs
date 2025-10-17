namespace Forex.Application.Features.Sales.SaleItems.Commands;

public class SaleItemCommand
{
    public int TypeCount { get; set; }  // 1 ta razmer (6 ta ayoq kiyim) razmerlar soni sotildi
    public int Count { get; set; }    // (1 ta ayoq kiyim) 1 qatorda jami 50 ta ayoq kiyim sotildi
    public decimal Price { get; set; }  // 1 ta mahsulotning narxi
    public decimal Amount { get; set; }  // 1 ta savdoning bir qatorining summasi

    public long ProductTypeId { get; set; } // 24-29, 30-35, 36-41 razmeri idsi
}