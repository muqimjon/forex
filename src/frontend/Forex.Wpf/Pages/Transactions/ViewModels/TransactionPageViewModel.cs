namespace Forex.Wpf.Pages.Transactions.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

public partial class TransactionPageViewModel : ViewModelBase
{
    private readonly ForexClient client;
    private readonly IMapper mapper;

    // Edit qilinayotgan tranzaksiyaning original qiymati
    private decimal _originalTransactionNetAmount = 0;

    public TransactionPageViewModel(ForexClient client, IMapper mapper)
    {
        this.client = client;
        this.mapper = mapper;

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(BeginDate) or nameof(EndDate))
                _ = LoadTransactionsAsync();
        };

        Transaction.PropertyChanged += OnTransactionPropertyChanged;

        _ = LoadDataAsync();
    }

    [ObservableProperty] private ObservableCollection<TransactionViewModel> transactions = [];
    [ObservableProperty] private ObservableCollection<UserViewModel> availableUsers = [];
    [ObservableProperty] private ObservableCollection<CurrencyViewModel> availableCurrencies = [];
    [ObservableProperty] private ObservableCollection<ShopAccountViewModel> availableShopAccounts = [];
    [ObservableProperty] private TransactionViewModel transaction = new();

    public static IEnumerable<PaymentMethod> AvailablePaymentMethods => Enum.GetValues<PaymentMethod>();
    [ObservableProperty] private DateTime? beginDate = DateTime.Today;
    [ObservableProperty] private DateTime? endDate = DateTime.Today;

    // UI-specific computed properties
    [ObservableProperty] private bool isExpense;
    [ObservableProperty] private decimal? totalAmountWithUserBalance;
    [ObservableProperty] private bool isDebtor;

    #region Load Data

    private async Task LoadDataAsync()
    {
        await Task.WhenAll(
            LoadShopCashes(),
            LoadUsersAsync(),
            LoadCurrenciesAsync(),
            LoadTransactionsAsync()
        );
    }

    private async Task LoadTransactionsAsync()
    {
        FilteringRequest request = new();

        DateTime begin = BeginDate?.Date ?? DateTime.Today;
        DateTime end = (EndDate?.Date ?? DateTime.Today).AddDays(1);

        request.Filters = new()
        {
            ["date"] =
            [
                $">={begin:yyyy-MM-dd}",
                $"<{end:yyyy-MM-dd}"
            ],
            ["user"] = ["include"],
            ["currency"] = ["include"],
            ["shopAccount"] = ["include"]
        };

        Response<List<TransactionResponse>> response = await client.Transactions.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            List<TransactionResponse> ordered = response.Data.OrderByDescending(t => t.Date).ToList();
            Transactions = mapper.Map<ObservableCollection<TransactionViewModel>>(ordered);

            // Income/Expense UI binding uchun
            foreach (var trans in Transactions)
            {
                if (trans.IsIncome)
                    trans.Income = trans.Amount;
                else
                    trans.Expense = trans.Amount;
            }
        }
        else
        {
            WarningMessage = response.Message ?? "Tranzaksiyalarni yuklashda xatolik.";
        }
    }

    private async Task LoadShopCashes()
    {
        Response<List<ShopResponse>> response = await client.Shops.GetAllAsync().Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            IEnumerable<ShopAccountResponse> accounts = response.Data.SelectMany(sh => sh.ShopAccounts);
            AvailableShopAccounts = mapper.Map<ObservableCollection<ShopAccountViewModel>>(accounts);
        }
        else
        {
            WarningMessage = response.Message ?? "Do'kon kassalarini yuklashda xatolik.";
        }
    }

    private async Task LoadCurrenciesAsync()
    {
        Response<List<CurrencyResponse>> response = await client.Currencies.GetAllAsync().Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            AvailableCurrencies = mapper.Map<ObservableCollection<CurrencyViewModel>>(response.Data);
            Transaction.Currency = AvailableCurrencies.FirstOrDefault(c => c.IsDefault)!;
            if (Transaction.Currency is not null)
                Transaction.ExchangeRate = Transaction.Currency.ExchangeRate;
        }
        else
        {
            WarningMessage = response.Message ?? "Valyuta turlarini yuklashda xatolik.";
        }
    }

    private async Task LoadUsersAsync()
    {
        Response<List<UserResponse>> response = await client.Users.GetAllAsync().Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            AvailableUsers = mapper.Map<ObservableCollection<UserViewModel>>(response.Data);
        }
        else
        {
            WarningMessage = response.Message ?? "Foydalanuvchilarni yuklashda xatolik.";
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task Submit()
    {
        if (!ValidateTransaction())
            return;

        // UI'dan backend modelga o'tkazish
        SyncTransactionFromUI();

        if (Transaction.Date.Date == DateTime.Today)
            Transaction.Date = DateTime.Now;

        TransactionRequest request = mapper.Map<TransactionRequest>(Transaction);

        if (IsEditing && Transaction.Id > 0)
        {
            var response = await client.Transactions.Update(request)
                .Handle(isLoading => IsLoading = isLoading);

            if (response.IsSuccess)
            {
                SuccessMessage = "Tranzaksiya muvaffaqiyatli yangilandi!";
                ResetTransaction();
            }
            else
            {
                ErrorMessage = response.Message ?? "Tranzaksiyani yangilashda xatolik yuz berdi.";
                return;
            }
        }
        else
        {
            var response = await client.Transactions.CreateAsync(request)
                .Handle(isLoading => IsLoading = isLoading);

            if (response.IsSuccess)
            {
                SuccessMessage = "To'lov muvaffaqiyatli amalga oshirildi.";
                ResetTransaction();
            }
            else
            {
                ErrorMessage = response.Message ?? "To'lovni amalga oshirishda xatolik yuz berdi.";
                return;
            }
        }

        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task Edit(TransactionViewModel selectedTransaction)
    {
        if (selectedTransaction is null)
            return;

        bool hasCurrentData = Transaction.Income.HasValue ||
                             Transaction.Expense.HasValue ||
                             Transaction.User is not null;

        if (hasCurrentData)
        {
            var result = MessageBox.Show(
                "Hozirgi kiritilgan ma'lumotlar o'chib ketadi. Davom etmoqchimisiz?",
                "Ogohlantirish",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
                return;
        }

        // PropertyChanged event'larni vaqtincha o'chirish
        Transaction.PropertyChanged -= OnTransactionPropertyChanged;

        try
        {
            // Original transaction qiymatini saqlash
            _originalTransactionNetAmount = (decimal)(selectedTransaction.Amount * selectedTransaction.ExchangeRate)!;

            // Ma'lumotlarni ko'chirish
            Transaction.Id = selectedTransaction.Id;
            Transaction.Date = selectedTransaction.Date;
            Transaction.IsIncome = selectedTransaction.IsIncome;
            Transaction.Amount = selectedTransaction.Amount;
            Transaction.ExchangeRate = selectedTransaction.ExchangeRate;
            Transaction.Discount = selectedTransaction.Discount;
            Transaction.PaymentMethod = selectedTransaction.PaymentMethod;
            Transaction.Description = selectedTransaction.Description;
            Transaction.DueDate = selectedTransaction.DueDate;

            // User'ni topish va o'rnatish
            Transaction.User = AvailableUsers.FirstOrDefault(u => u.Id == selectedTransaction.UserId)
                              ?? selectedTransaction.User;

            // Currency'ni topish va o'rnatish
            Transaction.Currency = AvailableCurrencies.FirstOrDefault(c => c.Id == selectedTransaction.CurrencyId)
                                  ?? selectedTransaction.Currency;

            // UI properties'ni o'rnatish
            if (Transaction.IsIncome)
            {
                Transaction.Income = Transaction.Amount;
                Transaction.Expense = null;
            }
            else
            {
                Transaction.Expense = Transaction.Amount;
                Transaction.Income = null;
            }

            // Edit rejimini yoqish
            IsEditing = true;

            // DataGrid'dan olib tashlash
            Transactions.Remove(selectedTransaction);
        }
        finally
        {
            // PropertyChanged event'ni qayta ulash
            Transaction.PropertyChanged += OnTransactionPropertyChanged;

            // Hisob-kitoblarni yangilash
            RecalculateAll();
        }
    }

    [RelayCommand]
    private async Task Delete(TransactionViewModel value)
    {
        if (value is null)
            return;

        var amount = value.IsIncome ? value.Income : value.Expense;
        var result = MessageBox.Show(
            $"Tranzaksiyani o'chirishni tasdiqlaysizmi?\n\nUser: {value.User?.Name}\nSumma: {amount:N2} {value.Currency?.Code}",
            "O'chirishni tasdiqlash",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.No)
            return;

        var response = await client.Transactions.Delete(value.Id)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            Transactions.Remove(value);
            SuccessMessage = "Tranzaksiya muvaffaqiyatli o'chirildi";
            await LoadDataAsync();
        }
        else
        {
            ErrorMessage = response.Message ?? "Tranzaksiyani o'chirishda xatolik";
        }
    }

    private void OnTransactionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Transaction.Income):
                OnIncomeChanged();
                break;
            case nameof(Transaction.Expense):
                OnExpenseChanged();
                break;
            case nameof(Transaction.Discount):
            case nameof(Transaction.User):
                RecalculateTotalAmountWithUserBalance();
                break;
            case nameof(Transaction.ExchangeRate):
                RecalculateAll();
                break;
            case nameof(Transaction.Currency):
                OnCurrencyChanged();
                break;
        }
    }

    #endregion

    #region Private Helpers

    private void ResetTransaction()
    {
        Transaction.PropertyChanged -= OnTransactionPropertyChanged;
        Transaction = new();
        Transaction.Currency = AvailableCurrencies.FirstOrDefault(c => c.IsDefault)!;
        if (Transaction.Currency is not null)
            Transaction.ExchangeRate = Transaction.Currency.ExchangeRate;
        Transaction.PropertyChanged += OnTransactionPropertyChanged;
        IsEditing = false;
        _originalTransactionNetAmount = 0;
        RecalculateAll();
    }

    private void SyncTransactionFromUI()
    {
        // UI'dan backend modelga sync qilish
        if (Transaction.Income.HasValue && Transaction.Income > 0)
        {
            Transaction.IsIncome = true;
            Transaction.Amount = Transaction.Income.Value;
        }
        else if (Transaction.Expense.HasValue && Transaction.Expense > 0)
        {
            Transaction.IsIncome = false;
            Transaction.Amount = Transaction.Expense.Value;
        }
    }

    private void OnIncomeChanged()
    {
        if (Transaction.Income is null || Transaction.Income == 0)
        {
            IsExpense = false;
        }
        else
        {
            IsExpense = false;
            Transaction.Expense = null;
        }

        RecalculateAll();
    }

    private void OnExpenseChanged()
    {
        if (Transaction.Expense is null || Transaction.Expense == 0)
        {
            IsExpense = false;
        }
        else
        {
            IsExpense = true;
            Transaction.Income = null;
            Transaction.Discount = null;
        }

        RecalculateAll();
    }

    private void OnCurrencyChanged()
    {
        if (Transaction.Currency is not null)
        {
            Transaction.Currency.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(Transaction.Currency.ExchangeRate))
                {
                    Transaction.ExchangeRate = Transaction.Currency.ExchangeRate;
                    RecalculateAll();
                }
            };

            Transaction.ExchangeRate = Transaction.Currency.ExchangeRate;
        }

        RecalculateAll();
    }

    private void RecalculateAll()
    {
        RecalculateTotalAmountWithUserBalance();
    }

    private void RecalculateTotalAmountWithUserBalance()
    {
        if (Transaction.User is null)
        {
            TotalAmountWithUserBalance = null;
            IsDebtor = false;
            return;
        }

        // Joriy tranzaksiya qiymati (IsIncome=true bo'lsa +, false bo'lsa -)
        decimal currentAmount = Transaction.Income ?? -(Transaction.Expense ?? 0);
        decimal exchangeRate = (decimal)(Transaction.ExchangeRate > 0 ? Transaction.ExchangeRate : 1);
        decimal currentNetAmount = currentAmount * exchangeRate;
        decimal discount = Transaction?.Discount ?? 0;

        decimal total;

        if (IsEditing && Transaction?.Id > 0)
        {
            total = (decimal)Transaction.User.Balance! - _originalTransactionNetAmount + currentNetAmount + discount;
        }
        else
        {
            // Add rejimida: oddiy qo'shamiz
            total = (decimal)Transaction.User.Balance! + currentNetAmount + discount;
        }

        TotalAmountWithUserBalance = total;
        IsDebtor = total < 0;
    }

    private bool ValidateTransaction()
    {
        if (!Transaction.Income.HasValue && !Transaction.Expense.HasValue)
        {
            WarningMessage = "Kirim yoki Chiqim kiritilishi kerak!";
            return false;
        }

        if (TotalAmountWithUserBalance < 0 &&
            (!Transaction.DueDate.HasValue || Transaction.DueDate < DateTime.Now))
        {
            WarningMessage = "To'lov muddati kiritilmagan yoki noto'g'ri formatda!";
            return false;
        }

        return true;
    }

    #endregion
}