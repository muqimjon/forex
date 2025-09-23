namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;

public partial class ContainerEntryViewModel : ViewModelBase
{
    [ObservableProperty] private long senderId;
    [ObservableProperty] private long invoiceId;
    [ObservableProperty] private long count;
    [ObservableProperty] private decimal price;
}
