namespace Forex.Wpf.ViewModels;

using Forex.Wpf.Pages.Common;

public class CustomerSaleViewModel : ViewModelBase
{
    public int RowNumber { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int ReadyCount { get; set; }
    public int MixedCount { get; set; }
    public int EvaCount { get; set; }
    public int TotalCount => ReadyCount + MixedCount + EvaCount;
}