using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.Pages.Products.ViewModels;
using System.Collections.ObjectModel;

public partial class UserViewModel : ViewModelBase
{
    [ObservableProperty] private UserResponse? selectedUser;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private string address = string.Empty;
    [ObservableProperty] private decimal balance;
    [ObservableProperty] private ObservableCollection<ProductViewModel> employeeProducts = [];


    partial void OnSelectedUserChanged(UserResponse? value)
    {
        if (value != null)
        {
            name = value.Name;
            Phone = value.Phone ?? string.Empty;
            Address = value.Address ?? string.Empty;
            Balance = value.Accounts?.FirstOrDefault()?.Balance ?? 0;
        }
        else
        {
            name = string.Empty;
            Phone = string.Empty;
            Address = string.Empty;
            Balance = 0;
        }
    }
}
