namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;

public partial class SemiProductViewModel : ViewModelBase
{
    [ObservableProperty] private long id;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private int code;
    [ObservableProperty] private string measure = string.Empty;
    [ObservableProperty] private string? photoPath;

    [ObservableProperty] private decimal quantity;
    [ObservableProperty] private decimal costPrice;
    [ObservableProperty] private decimal costDelivery;
    [ObservableProperty] private decimal transferFee;
    [ObservableProperty] private bool isEditing;
}
