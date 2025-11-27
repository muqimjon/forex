namespace Forex.Wpf.Pages.ShopCashes.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Wordprocessing;
using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System.Collections.ObjectModel;

public partial class PaymentPageViewModel : ViewModelBase
{
    private readonly ForexClient client;
    private readonly IMapper mapper;

    public PaymentPageViewModel(ForexClient client, IMapper mapper)
    {
        this.client = client;
        this.mapper = mapper;

        this.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(BeginDate) or nameof(EndDate))
                _ = LoadTransactionsAsync();
        };

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
        var request = new FilteringRequest();

        // Sana bo‘yicha filter
        var begin = BeginDate?.Date ?? DateTime.Today;
        var end = (EndDate?.Date ?? DateTime.Today).AddDays(1);

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

        var response = await client.Transactions.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            var ordered = response.Data.OrderByDescending(t => t.Date).ToList();
            Transactions = mapper.Map<ObservableCollection<TransactionViewModel>>(ordered);
        }
        else
        {
            WarningMessage = response.Message ?? "Tranzaksiyalarni yuklashda xatolik.";
        }
    }

    private async Task LoadShopCashes()
    {
        var response = await client.Shops.GetAllAsync().Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            var accounts = response.Data.SelectMany(sh => sh.ShopAccounts);
            AvailableShopAccounts = mapper.Map<ObservableCollection<ShopAccountViewModel>>(accounts);
        }
        else
        {
            WarningMessage = response.Message ?? "Do'kon kassalarini yuklashda xatolik.";
        }
    }

    private async Task LoadCurrenciesAsync()
    {
        var response = await client.Currencies.GetAllAsync().Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            AvailableCurrencies = mapper.Map<ObservableCollection<CurrencyViewModel>>(response.Data);
            Transaction.Currency = AvailableCurrencies.FirstOrDefault(c => c.IsDefault)!;
        }
        else
        {
            WarningMessage = response.Message ?? "Valyuta turlarini yuklashda xatolik.";
        }
    }

    private async Task LoadUsersAsync()
    {
        var response = await client.Users.GetAllAsync().Handle(isLoading => IsLoading = isLoading);

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

        if (Transaction.Date.Date == DateTime.Today)
            Transaction.Date = DateTime.Now;

        var request = mapper.Map<TransactionRequest>(Transaction);
        var response = await client.Transactions.CreateAsync(request).Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            SuccessMessage = "To'lov muvaffaqiyatli amalga oshirildi.";
            Transaction = new();
            await LoadDataAsync();
        }
        else WarningMessage = response.Message ?? "To'lovni amalga oshirishda xatolik yuz berdi.";
    }

    private bool ValidateTransaction()
    {
        if (Transaction.TotalAmountWithUserBalance < 0 &&
            (!Transaction.DueDate.HasValue || Transaction.DueDate < DateTime.Now))
        {
            WarningMessage = "To'lov muddati kiritilmagan yoki noto'g'ri formatda!";
            return false;
        }

        return true;
    }

    #endregion
}