namespace Forex.Wpf.ViewModels;

using Forex.Wpf.Pages.Common;

public partial class ProductionItemViewModel : ViewModelBase
{
    public int RowNumber { get; set; }
    public DateTime Date { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int BundleCount { get; set; }     // Qop soni
    public int BundleItemCount { get; set; } // Donasi
    public int TotalCount { get; set; }      // Jami
    public string ProductionType { get; set; } = string.Empty;
}