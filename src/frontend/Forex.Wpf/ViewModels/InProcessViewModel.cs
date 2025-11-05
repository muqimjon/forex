namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;

public partial class InProcessViewModel : ViewModelBase
{
    [ObservableProperty] private long id;
    [ObservableProperty] private decimal quantity;
    [ObservableProperty] private long semiProductId;
    [ObservableProperty] private SemiProductViewModel semiProduct = default!;
}
