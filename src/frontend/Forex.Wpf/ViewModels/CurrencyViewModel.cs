namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;

public partial class CurrencyViewModel : ViewModelBase
{
    public long Id;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string code = string.Empty;
    [ObservableProperty] private string symbol = string.Empty;
    [ObservableProperty] private decimal exchangeRate;
    [ObservableProperty] private bool isDefault;
    [ObservableProperty] private bool isActive;
    [ObservableProperty] private bool isEditable;
    [ObservableProperty] private int position;
}
