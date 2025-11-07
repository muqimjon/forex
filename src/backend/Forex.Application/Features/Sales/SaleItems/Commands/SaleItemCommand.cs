namespace Forex.Application.Features.Sales.SaleItems.Commands;

public class SaleItemCommand
{
    public int BundleCount { get; set; }  // 1 ta razmer (50 ta ayoq kiyim) razmerlar soni sotildi
    public decimal UnitPrice { get; set; }  // 1 ta mahsulotning narxi
    public decimal Amount { get; set; }  // 1 ta savdoning bir qatorining summasi

    public long ProductTypeId { get; set; } // 24-29, 30-35, 36-41 razmeri idsi
}