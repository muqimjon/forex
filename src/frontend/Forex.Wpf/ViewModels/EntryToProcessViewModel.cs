namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;

public partial class EntryToProcessViewModel : ViewModelBase
{
    public long Id { get; set; }
    public long InProcessId { get; set; }
    [ObservableProperty] private decimal quantity;
    [ObservableProperty] private long productTypeId;
    [ObservableProperty] private ProductTypeViewModel productType = default!;
    [ObservableProperty] private InProcessViewModel inProcess = default!;
}