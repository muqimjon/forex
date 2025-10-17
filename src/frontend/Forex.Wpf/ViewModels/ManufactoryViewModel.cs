namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;

public partial class ManufactoryViewModel : ViewModelBase
{
    [ObservableProperty] private long id;
    [ObservableProperty] private string name = string.Empty;
}