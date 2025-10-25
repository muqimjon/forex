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
    [ObservableProperty] private ObservableCollection<ProductEntryViewModel> preparedProducts = [];
    private UserViewModel? selected;

    // UI qismi uchun
    [ObservableProperty] private decimal? balance;
    [ObservableProperty] private decimal? discount;

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
                PreparedProducts = new ObservableCollection<ProductEntryViewModel>(value.PreparedProducts ?? []);
            }
        }
    }

    partial void OnAccountsChanged(ObservableCollection<UserAccountViewModel> value)
    {
        CalculateBalance();
        CalculateDiscount();
    }

    #endregion Property Changes

    #region Private Helpers

    private void CalculateBalance()
    {
        if (Accounts.Any())
            Balance = Accounts
                .Where(x => x.Currency is not null && x.Currency.Code == "UZS")
                .Sum(x => x.Balance);
    }

    private void CalculateDiscount()
    {
        if (Accounts.Any())
            Discount = Accounts
                .Where(x => x.Currency is not null && x.Currency.Code == "UZS")
                .Sum(x => x.Discount);
    }

    #endregion Private Helpers
}
