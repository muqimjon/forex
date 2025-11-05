namespace Forex.Wpf.Pages.Processes.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;

public partial class EntryToProcessByProductViewModel : ViewModelBase
{
    public EntryToProcessByProductViewModel()
    {
        Product = new();
    }

    [ObservableProperty] private decimal quantity;
    [ObservableProperty] private ProductViewModel product = default!;
}