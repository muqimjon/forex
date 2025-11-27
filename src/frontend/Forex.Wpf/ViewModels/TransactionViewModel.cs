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
    [ObservableProperty] private DateTime? dueDate;
    [ObservableProperty] private bool isDebtor;

    public long ShopId;
    public long UserId;
    public long CurrencyId;

    [ObservableProperty] private UserViewModel user = default!;
    [ObservableProperty] private CurrencyViewModel currency = default!;

    // UI Properties
    [ObservableProperty] private decimal? income;
    [ObservableProperty] private decimal? expense;
    [ObservableProperty] private bool isExpense;
    [ObservableProperty] private decimal? totalAmountWithUserBalance;
    [ObservableProperty] private decimal? netAmount;

    #region Property Changes

    partial void OnNetAmountChanged(decimal? value) => RecalculateTotalAmountWithUserBalance();
    partial void OnDiscountChanged(decimal? value) => RecalculateTotalAmountWithUserBalance();
    partial void OnUserChanged(UserViewModel value) => RecalculateTotalAmountWithUserBalance();
    partial void OnAmountChanged(decimal? value) => ReCalculateNetAmount();
    partial void OnExchangeRateChanged(decimal? value) => ReCalculateNetAmount();
    partial void OnExpenseChanged(decimal? value) => ChangeTansactionType(-Expense);
    partial void OnIncomeChanged(decimal? value) => ChangeTansactionType(Income);
    
    partial void OnCurrencyChanged(CurrencyViewModel value)
    {
        if (value is not null)
            value.PropertyChanged += OnCurrencyPropertyChanged;
    }

    private void OnCurrencyPropertyChanged(object? sender, PropertyChangedEventArgs e) => ReCalculateNetAmount();

    #endregion Property Changes

    #region Private Helpers

    private void ChangeTansactionType(decimal? value)
    {
        if (value is null || value == 0) IsIncome = IsExpense = false;
        else IsExpense = !(IsIncome = value > 0);

        if (IsExpense) Discount = default;

        Amount = value;
    }

    private void RecalculateTotalAmountWithUserBalance()
    {
        if (User is null)
        {
            TotalAmountWithUserBalance = null;
            IsDebtor = false;
            return;
        }

        var total = User.Balance + (NetAmount ?? 0) + (Discount ?? 0);
        TotalAmountWithUserBalance = total;
        IsDebtor = total < 0;
    }

    private void ReCalculateNetAmount() => NetAmount = Amount * Currency?.ExchangeRate;

    #endregion
}