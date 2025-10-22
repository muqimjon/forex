namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

public partial class UserViewModel : ViewModelBase
{
    [ObservableProperty] private long id;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string address = string.Empty;
    [ObservableProperty] private string description = string.Empty;

    [ObservableProperty] private ObservableCollection<UserAccountViewModel> accounts = [];
    [ObservableProperty] private ObservableCollection<ProductTypeViewModel> preparedProductTypes = [];
    private UserViewModel? selected;

    // UI qismi uchun
    [ObservableProperty] private decimal? balance;

    #region Property Changes

    public UserViewModel? Selected
    {
        get => selected;
        set
        {
            if (SetProperty(ref selected, value) && value != null)
            {
                Id = value.Id;
                Name = value.Name;
                Phone = value.Phone;
                Email = value.Email;
                Address = value.Address;
                Description = value.Description;
                Accounts = new ObservableCollection<UserAccountViewModel>(value.Accounts ?? []);
                PreparedProductTypes = new ObservableCollection<ProductTypeViewModel>(value.PreparedProductTypes ?? []);
            }
        }
    }

    #endregion Property Changes

    #region Private Helpers

    partial void OnAccountsChanged(ObservableCollection<UserAccountViewModel> value)
    {
        if (accounts.Any())
            Balance = Accounts
                .Where(x => x.Currency is not null && x.Currency.Code == "UZS")
                .Sum(x => x.Balance);
    }

    #endregion Private Helpers
}
