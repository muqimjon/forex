namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;

public partial class UnitMeasuerViewModel : ViewModelBase
{
    [ObservableProperty] private long id;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string symbol = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private bool isDefault;
    [ObservableProperty] private bool isActive;
    [ObservableProperty] private int position;
}