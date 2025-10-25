namespace Forex.Wpf.Pages.Sales.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

public partial class SalePageViewModel : ViewModelBase
{
    private readonly ForexClient client;
    private readonly IMapper mapper;
    public SalePageViewModel(ForexClient client, IMapper mapper)
    {
        this.client = client;
        this.mapper = mapper;

        _ = LoadData();
    }

    // 🗓 Sana
    [ObservableProperty] private DateTime operationDate = DateTime.Now;
    [ObservableProperty] private decimal? totalAmount;
    [ObservableProperty] private decimal? finalAmount;
    [ObservableProperty] private decimal? totalAmountWithUserBalance;
    [ObservableProperty] private string note = string.Empty;

    [ObservableProperty] private SaleItemViewModel currentSaleItem = new();
    [ObservableProperty] private ObservableCollection<SaleItemViewModel> saleItems = [];

    [ObservableProperty] private UserViewModel? customer;
    [ObservableProperty] private ObservableCollection<UserViewModel> availableCustomers = [];
    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];


    #region Load Data

    private async Task LoadData()
    {
        await LoadUsersAsync();
        await LoadProductsAsync();
    }

    public async Task LoadUsersAsync()
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["role"] = ["mijoz"],
                ["accounts"] = ["include:currency"]
            }
        };

        var response = await client.Users.Filter(request).Handle();
        if (response.IsSuccess)
            AvailableCustomers = mapper.Map<ObservableCollection<UserViewModel>>(response.Data!);
        else ErrorMessage = response.Message ?? "Mahsulot turlarini yuklashda xatolik.";
    }

    public async Task LoadProductsAsync()
    {
        var response = await client.Products.GetAll().Handle(isLoading => IsLoading = isLoading);
        if (response.IsSuccess)
            AvailableProducts = mapper.Map<ObservableCollection<ProductViewModel>>(response.Data!);
        else ErrorMessage = response.Message ?? "Foydalanuvchilarni yuklashda xatolik.";
    }

    public async Task LoadProductTypessAsync(ProductViewModel product)
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["productid"] = [product.Id.ToString()]
            }
        };

        var response = await client.ProductTypes.Filter(request).Handle(isLoading => IsLoading = isLoading);
        if (response.IsSuccess)
            CurrentSaleItem.Product.ProductTypes = mapper.Map<ObservableCollection<ProductTypeViewModel>>(response.Data!);
        else ErrorMessage = response.Message ?? "Mahsulot turlarini yuklashda xatolik.";
    }

    #endregion Load Data

    #region Commands

    [RelayCommand]
    private void Add()
    {
        if (CurrentSaleItem is null || CurrentSaleItem.ProductType.Count <= 0)
        {
            WarningMessage = "Mahsulot tanlanmagan yoki miqdor noto‘g‘ri!";
            return;
        }

        SaleItemViewModel item = new()
        {
            Product = CurrentSaleItem.Product,
            ProductType = CurrentSaleItem.ProductType,
            BundleCount = CurrentSaleItem.BundleCount,
            UnitPrice = CurrentSaleItem.UnitPrice,
            Amount = CurrentSaleItem.Amount,
            TotalCount = CurrentSaleItem.TotalCount,
        };

        item.PropertyChanged += SaleItemPropertyChanged;
        SaleItems.Add(item);

        CurrentSaleItem = new SaleItemViewModel();
        RecalculateTotals();
    }

    [RelayCommand]
    private async Task Submit()
    {
        if (SaleItems.Count == 0)
        {
            WarningMessage = "Hech qanday mahsulot kiritilmagan!";
            return;
        }

        SaleRequest request = new()
        {
            Date = OperationDate,
            CustomerId = Customer?.Id ?? 0,
            TotalAmount = FinalAmount ?? 0,
            Note = Note,
            SaleItems = [.. SaleItems.Select(item => new SaleItemRequest
            {
                ProductTypeId = item.ProductType.Id,
                BundleCount = (int)item.BundleCount!,
                UnitPrice = (decimal)item.UnitPrice!,
                Amount = (decimal)item.Amount!
            })]
        };

        var response = await client.Sales.Create(request).Handle(isLoading => IsLoading = isLoading);
        if (response.IsSuccess)
            SuccessMessage = $"Savdo muvaffaqiyatli yuborildi. Mahsulotlar soni: {SaleItems.Count}";
        else ErrorMessage = response.Message ?? "Savdoni ro'yxatga olishda xatolik!";
    }

    #endregion Commands

    #region Property Changes

    partial void OnCustomerChanged(UserViewModel? value) => RecalculateTotalAmountWithUserBalance();

    private void SaleItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SaleItemViewModel.Amount))
        {
            RecalculateTotals();
        }
    }

    partial void OnFinalAmountChanged(decimal? value)
    {
        if (Customer is not null)
            TotalAmountWithUserBalance = Customer.Balance - FinalAmount;
    }

    #endregion Property Changes

    #region Private Helpers

    private void RecalculateTotals()
    {
        TotalAmount = SaleItems.Sum(x => x.Amount);
        FinalAmount = TotalAmount;
    }

    private void RecalculateTotalAmountWithUserBalance()
    {
        if (Customer is not null)
            TotalAmountWithUserBalance = Customer.Balance - TotalAmount;
    }

    #endregion Private Helpers
}