namespace Forex.Wpf.ViewModels;

public class SaleHistoryItemViewModel
{
    public DateTime Date { get; set; }
    public string Customer { get; set; } = default!;

    // ProductType → Product
    public string Code { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public string Type { get; set; } = default!;     // Razmer
    public uint BundleCount { get; set; }
    public int BundleItemCount { get; set; }

    // SaleItem
    public uint TotalCount { get; set; }
    public string UnitMeasure { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}
