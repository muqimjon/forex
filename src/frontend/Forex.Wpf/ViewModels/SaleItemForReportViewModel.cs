namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;

public partial class SaleItemForReportViewModel : ViewModelBase
{
    public int Id;
    [ObservableProperty] private DateTime? date;
    [ObservableProperty] private string? customer;
    [ObservableProperty] private int? code;
    [ObservableProperty] private string? name;
    [ObservableProperty] private string? type;
    [ObservableProperty] private int? bundleItemCount;
    [ObservableProperty] private int? bundleCount;
    [ObservableProperty] private int? totalCount;
    [ObservableProperty] private string? unitMeusure;
    [ObservableProperty] private decimal? amount;
}
