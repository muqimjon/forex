namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService.Enums;
using Forex.Wpf.Pages.Common;

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

    #region Property Changes

    partial void OnAmountChanged(decimal? value) => RecalculateTotalAmountWithUserBalance();

    partial void OnDiscountChanged(decimal? value) => RecalculateTotalAmountWithUserBalance();

    partial void OnCurrencyChanged(CurrencyViewModel value) => ExchangeRate = value?.ExchangeRate;

    partial void OnExchangeRateChanged(decimal? value) => RecalculateAmount();

    partial void OnUserChanged(UserViewModel value) => RecalculateTotalAmountWithUserBalance();

    partial void OnExpenseChanged(decimal? value)
    {
        if (!value.HasValue || value <= 0)
        {
            IsExpense = false;
            RecalculateAmount();
            return;
        }

        Discount = null;
        IsIncome = false;
        IsExpense = true;
        RecalculateAmount();
    }

    partial void OnIncomeChanged(decimal? value)
    {
        if (!value.HasValue || value <= 0)
        {
            IsIncome = false;
            RecalculateAmount();
            return;
        }

        IsExpense = false;
        IsIncome = true;
        RecalculateAmount();
    }

    #endregion

    #region Private Helpers

    private void RecalculateAmount()
    {
        if (IsIncome)
            Amount = (Income ?? 0) * (ExchangeRate ?? 0);
        else
            Amount = -(Expense ?? 0) * (ExchangeRate ?? 0);

        RecalculateTotalAmountWithUserBalance();
    }

    private void RecalculateTotalAmountWithUserBalance()
    {
        if (User is null)
        {
            TotalAmountWithUserBalance = null;
            IsDebtor = false;
            return;
        }

        var total = User.Balance + (Amount ?? 0) + (Discount ?? 0);
        TotalAmountWithUserBalance = total;
        IsDebtor = total < 0;
    }

    #endregion
}