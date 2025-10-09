namespace Forex.Wpf.Pages.Sales.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;


public partial class UserViewModel : ViewModelBase
{
    [ObservableProperty] private long id;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string address = string.Empty;
    [ObservableProperty] private string description = string.Empty;

}