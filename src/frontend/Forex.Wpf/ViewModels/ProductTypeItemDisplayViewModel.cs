namespace Forex.Wpf.ViewModels;

public class ProductTypeItemDisplayViewModel
{
    public long Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string MeasureName { get; set; } = string.Empty;
}
