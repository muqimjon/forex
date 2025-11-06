namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public partial class InProcessViewModel : ViewModelBase
{
    public long Id { get; set; }
    [ObservableProperty] private decimal quantity;
    [ObservableProperty] private long productTypeId;
    [ObservableProperty] private ProductTypeViewModel productType = default!;

    [ObservableProperty] private ObservableCollection<EntryToProcessViewModel> entryToProcesses = [];
}
