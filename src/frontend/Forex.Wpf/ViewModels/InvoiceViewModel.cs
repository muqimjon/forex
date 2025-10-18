namespace Forex.Wpf.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.Wpf.Pages.Common;
using MapsterMapper;
using System.Collections.ObjectModel;

public partial class InvoiceViewModel : ViewModelBase
{
    private readonly ForexClient client;
    private readonly IMapper mapper;
    public InvoiceViewModel(ForexClient client, IMapper mapper)
    {
        this.client = client;
        this.mapper = mapper;
        _ = LoadPageAsync();
    }

    #region Loading Data

    private async Task LoadPageAsync()
    {
        await LoadUsersAsync();
        await LoadManufactoriesAsync();
        await LoadCurrenciesAsync();
    }

    #region Manufactory

    private async Task LoadManufactoriesAsync()
    {
        var response = await client.Manufactories.GetAll()
            .Handle(isLoading => IsLoading = isLoading);
        if (!response.IsSuccess)
        {
            ErrorMessage = response.Message ?? "Foydalanuvchilarni yuklashda noma'lum xatolik yuz berdi.";
            return;
        }

        Manufactories = mapper.Map<ObservableCollection<ManufactoryViewModel>>(response.Data!);
        Manufactory = Manufactories.FirstOrDefault() ?? new();
    }
    [ObservableProperty] private ObservableCollection<ManufactoryViewModel> manufactories = default!;


    #endregion Manufactory

    #region User

    private async Task LoadUsersAsync()
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["role"] = ["in:Taminotchi,Vositachi"]
            }
        };

        var response = await client.Users.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (!response.IsSuccess)
        {
            ErrorMessage = response.Message ?? "Foydalanuvchilarni yuklashda noma'lum xatolik yuz berdi.";
            return;
        }

        Suppliers = mapper.Map<ObservableCollection<UserViewModel>>(response.Data!.Where(u => u.Role == UserRole.Taminotchi));
        Agents = mapper.Map<ObservableCollection<UserViewModel>>(response.Data!.Where(u => u.Role == UserRole.Vositachi));
    }
    [ObservableProperty] private ObservableCollection<UserViewModel> suppliers = default!;
    [ObservableProperty] private ObservableCollection<UserViewModel> agents = default!;

    #endregion User

    #region Currency

    private async Task LoadCurrenciesAsync()
    {
        var response = await client.Currency.GetAll().Handle();
        if (!response.IsSuccess)
        {
            ErrorMessage = response.Message;
            return;
        }

        Currencies = mapper.Map<ObservableCollection<CurrencyViewModel>>(response.Data!);
        Currency = Currencies.FirstOrDefault(c => c.IsDefault)
            ?? Currencies.FirstOrDefault()
            ?? new();
    }
    [ObservableProperty] private ObservableCollection<CurrencyViewModel> currencies = default!;

    #endregion Currency

    #endregion Loading Data

    public long Id { get; set; }
    [ObservableProperty] private ManufactoryViewModel manufactory = default!;
    [ObservableProperty] private UserViewModel supplier = default!;
    [ObservableProperty] private UserViewModel? agent = default!;
    [ObservableProperty] private CurrencyViewModel currency = default!;
    [ObservableProperty] private DateTime date = DateTime.Now;
    [ObservableProperty] private string? number;
    [ObservableProperty] private decimal? costPrice;
    [ObservableProperty] private decimal? costDelivery;
    [ObservableProperty] private int? containerCount;
    [ObservableProperty] private decimal? pricePerUnitContainer;
    [ObservableProperty] private decimal? transferFee;
    [ObservableProperty] private bool viaMiddleman;

    [ObservableProperty] private decimal? totalAmount;
    [ObservableProperty] private string totalAmountWithCurrency = string.Empty;

    partial void OnCostDeliveryChanged(decimal? value) => ReCalculateTotal();
    partial void OnCostPriceChanged(decimal? value) => ReCalculateTotal();
    partial void OnTransferFeeChanged(decimal? value) => ReCalculateTotal();
    partial void OnViaMiddlemanChanged(bool value) => ReCalculateTotal();
    partial void OnContainerCountChanged(int? value) => ReCalculateTransferFee();
    partial void OnPricePerUnitContainerChanged(decimal? value) => ReCalculateTransferFee();
    partial void OnTotalAmountChanged(decimal? value) => RefreshAmount();
    partial void OnCurrencyChanged(CurrencyViewModel value) => RefreshAmount();

    private void RefreshAmount()
    {
        if (TotalAmount is not null)
            TotalAmountWithCurrency = $"{TotalAmount:N2} {Currency.Code}";
    }

    private void ReCalculateTransferFee()
    {
        TransferFee = ContainerCount * PricePerUnitContainer;
    }

    private void ReCalculateTotal()
    {
        if (CostDelivery is not null || CostPrice is not null || TransferFee is not null)
            if (ViaMiddleman) TotalAmount = (CostPrice ?? 0) + (CostDelivery ?? 0) + (TransferFee ?? 0);
            else TotalAmount = (CostPrice ?? 0) + (CostDelivery ?? 0);
    }
}

