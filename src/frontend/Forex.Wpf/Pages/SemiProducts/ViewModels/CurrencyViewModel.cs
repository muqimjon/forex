namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;

public partial class CurrencyViewModel : ViewModelBase
{
    public long Id { get; set; }
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string code = string.Empty;
    [ObservableProperty] private string symbol = string.Empty;
    [ObservableProperty] private decimal exchangeRate;
}