namespace Forex.Wpf.Pages.ShopCashes.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System;
using System.Collections.ObjectModel;

public partial class PaymentPageViewModel : ViewModelBase
{
    public ForexClient client;
    public IMapper mapper;

    public PaymentPageViewModel(ForexClient client, IMapper mapper)
    {
        this.client = client;
        this.mapper = mapper;

        _ = LoadDataAsync();
    }

    [ObservableProperty] private ObservableCollection<TransactionViewModel> transactions = [];


    [ObservableProperty] private ObservableCollection<UserViewModel> availableUsers = [];
    [ObservableProperty] private ObservableCollection<CurrencyViewModel> availableCurrencies = [];
    [ObservableProperty] private ObservableCollection<ShopAccountViewModel> availableShopAccounts = [];
    public static IEnumerable<PaymentMethod> AvailablePaymentMethods => Enum.GetValues<PaymentMethod>().Cast<PaymentMethod>();

    [ObservableProperty] private TransactionViewModel transaction = new();


    #region Load Data

    private async Task LoadDataAsync()
    {
        await LoadShopCashes();
        await LoadUsersAsync();
        await LoadCurrenciesAsync();
        await LoadTransactionsAsync();
    }

    private async Task LoadTransactionsAsync()
    {
        var response = await client.Transactions.GetAll().Handle(isLoading => IsLoading = isLoading);
        if (response.IsSuccess)
            Transactions = mapper.Map<ObservableCollection<TransactionViewModel>>(response.Data);
        else WarningMessage = response.Message ?? "Tranzaksiyalarni yuklashda xatolik.";
    }

    private async Task LoadShopCashes()
    {
        var response = await client.Shops.GetAllAsync().Handle(isLoading => IsLoading = isLoading);
        if (response.IsSuccess)
            AvailableShopAccounts = mapper.Map<ObservableCollection<ShopAccountViewModel>>(response.Data.SelectMany(sh => sh.ShopAccounts));
        else WarningMessage = response.Message ?? "Do'kon kassalarini yuklashda xatolik.";
    }

    private async Task LoadCurrenciesAsync()
    {
        var response = await client.Currencies.GetAllAsync().Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            AvailableCurrencies = mapper.Map<ObservableCollection<CurrencyViewModel>>(response.Data);
            Transaction.Currency = AvailableCurrencies.FirstOrDefault(c => c.IsDefault)!;
        }
        else WarningMessage = response.Message ?? "Valyuta turlarini yuklashda xatolik.";
    }

    private async Task LoadUsersAsync()
    {
        var response = await client.Users.GetAllAsync().Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
            AvailableUsers = mapper.Map<ObservableCollection<UserViewModel>>(response.Data);
        else WarningMessage = response.Message ?? "Foydalanuvchilarni yuklashda xatolik.";
    }

    #endregion Load Data

    #region Commands

    [RelayCommand]
    private async Task Submit()
    {
        var request = mapper.Map<TransactionRequest>(Transaction);
        var response = await client.Transactions.CreateAsync(request).Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            SuccessMessage = "To'lov muvaffaqiyatli amalga oshirildi.";
            Transaction = new();
            await LoadDataAsync();
        }
        else WarningMessage = response.Message ?? "To'lovni amalga oshirishda xatolik yuz berdi.";

        #endregion Commands
    }
}