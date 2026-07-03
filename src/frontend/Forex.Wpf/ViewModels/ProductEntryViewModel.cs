namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService.Enums;
using Forex.Wpf.Pages.Common;

public partial class ProductEntryViewModel : ViewModelBase
{
    public long Id { get; set; }

    [ObservableProperty] private int? count;
    [ObservableProperty] private int? availableCount;
    [ObservableProperty] private ProductionOrigin? productionOrigin;
    [ObservableProperty] private string productionOriginName = string.Empty;
    [ObservableProperty] private int? bundleItemCount;
    [ObservableProperty] private int? packItemCount;
    [ObservableProperty] private int? bundleCount;
    [ObservableProperty] private decimal? unitPrice;
    [ObservableProperty] private ProductTypeViewModel? productType;
    [ObservableProperty] private DateTime date = DateTime.Today;
    [ObservableProperty] private decimal? costPrice;
    [ObservableProperty] private decimal? preparationCostPerUnit;
    [ObservableProperty] private decimal? totalAmount;
}