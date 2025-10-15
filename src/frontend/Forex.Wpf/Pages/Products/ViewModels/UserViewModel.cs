using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public partial class UserViewModel : ViewModelBase
{
    [ObservableProperty] private UserResponse? selectedEmployee;
    [ObservableProperty] private ObservableCollection<UserResponse> employees = new();
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private string address = string.Empty;
    [ObservableProperty] private decimal balance;

    partial void OnSelectedEmployeeChanged(UserResponse? value)
    {
        if (value != null)
        {
            Phone = value.Phone ?? string.Empty;
            Address = value.Address ?? string.Empty;
            Balance = value.Accounts?.FirstOrDefault()?.Balance ?? 0;
        }
        else
        {
            Phone = string.Empty;
            Address = string.Empty;
            Balance = 0;
        }
    }
}
