namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService.Enums;
using Forex.Wpf.Pages.Common;
using System.ComponentModel;

public partial class TransactionViewModel : ViewModelBase
{
    public long Id;
    [ObservableProperty] private decimal? amount;
    [ObservableProperty] private decimal? exchangeRate;
    [ObservableProperty] private decimal? discount;
    [ObservableProperty] private PaymentMethod paymentMethod;
    [ObservableProperty] private bool isIncome;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private DateTime date = DateTime.Now;

    public long ShopId;
    public long UserId;
    [ObservableProperty] private UserViewModel user = default!;
    public long CurrencyId;
    [ObservableProperty] private CurrencyViewModel currency = default!;

    // For UI
    [ObservableProperty] private decimal? income;
    [ObservableProperty] private decimal? expense;
    [ObservableProperty] private bool isExpense;
    [ObservableProperty] private decimal? totalAmountWithUserBalance;
    [ObservableProperty] private DateTime dueDate = DateTime.Now.AddDays(1);


    #region Property Changes

    partial void OnAmountChanged(decimal? value) => RecalculateTotalAmountWithUserBalance();
    partial void OnDiscountChanged(decimal? value) => RecalculateTotalAmountWithUserBalance();
    partial void OnCurrencyChanged(CurrencyViewModel value) => ExchangeRate = value.ExchangeRate;

    partial void OnExpenseChanged(decimal? value)
    {
        if (value <= 0)
        {
            IsExpense = default;
            return;
        }

        Amount = -value;
        IsIncome = default;
        IsExpense = true;
    }

    partial void OnIncomeChanged(decimal? value)
    {
        if (value <= 0)
        {
            IsIncome = default;
            return;
        }

        Amount = value;
        IsExpense = default;
        IsIncome = true;
    }

    partial void OnUserChanged(UserViewModel value) => value.PropertyChanged += UserPropertyChanged;

    private void UserPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(UserViewModel.Balance))
            RecalculateTotalAmountWithUserBalance();
    }

    #endregion Property Changes

    #region Private Helpers

    private void RecalculateTotalAmountWithUserBalance()
    {
        if (User is not null && (Amount is not null || Discount is not null))
        {
            TotalAmountWithUserBalance = User.Balance + (Amount ?? 0) + (Discount ?? 0);
        }
        else
        {
            TotalAmountWithUserBalance = default;
        }
    }

    #endregion Private Helpers
}