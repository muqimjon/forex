namespace Forex.Wpf.Pages.Reports.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.Pages.Sales.ViewModels;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System.Collections.ObjectModel;

public partial class ReportsPageViewModel : ViewModelBase
{
    public ForexClient client;
    public IMapper mapper;
    public ReportsPageViewModel(ForexClient client, IMapper mapper)
    {
        this.mapper = mapper;
        this.client = client;
        _ = LoadDataAsync();
    }

    public ObservableCollection<SaleHistoryItemViewModel> SalesHistory { get; set; }
    = new ObservableCollection<SaleHistoryItemViewModel>();


    [ObservableProperty] private ObservableCollection<SaleItemForReportViewModel> saleItems = [];
    [ObservableProperty] private ObservableCollection<SaleItemForReportViewModel> filteredSaleItems = [];
    

    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];
    private ProductViewModel? selectedProduct;
    private ProductViewModel? selectedCode;

    [ObservableProperty] private ObservableCollection<UserViewModel> availableCustomers = [];
    private UserViewModel? selectedCustomer;

    private async Task LoadDataAsync()
    {
        await LoadCustomersAsync();
        await LoadProductsAsync();
        await LoadSaleHistoryAsync();
    }
    public async Task LoadSaleHistoryAsync()
    {
        var response = await client.Sales.GetAll().Handle(isLoading => IsLoading = isLoading);

        if (!response.IsSuccess)
        {
            ErrorMessage = response.Message ?? "Savdo ma'lumotlarini yuklashda xatolik.";
            return;
        }

        SalesHistory.Clear();

        LoadSales(response.Data);
    }

    public void LoadSales(IEnumerable<SaleResponse> sales)
    {
        SalesHistory.Clear();

        foreach (var sale in sales)
        {
            if (sale.SaleItems == null) continue;

            foreach (var item in sale.SaleItems)
            {
                var productType = item.ProductType;
                var product = productType?.Product;

                SalesHistory.Add(new SaleHistoryItemViewModel
                {
                    Date = sale.Date,
                    Customer = sale.Customer?.Name ?? "-",

                    Code = product?.Code ?? "-",
                    ProductName = product?.Name ?? "-",
                    Type = productType?.Type ?? "-",
                    BundleItemCount = productType?.BundleItemCount ?? 0,

                    TotalCount = item.TotalCount,
                    UnitMeasure = product?.UnitMeasure?.Name ?? "-",
                    UnitPrice = item.UnitPrice,
                    Amount = item.Amount
                });
            }
        }
    }

    private async Task LoadCustomersAsync()
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["role"] = ["mijoz"],
                ["accounts"] = ["include:currency"]
            }
        };

        var response = await client.Users.Filter(request).Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
            AvailableCustomers = mapper.Map<ObservableCollection<UserViewModel>>(response.Data);
        else WarningMessage = response.Message ?? "Foydalanuvchilarni yuklashda xatolik.";
    }

    public async Task LoadProductsAsync()
    {
        var response = await client.Products.GetAll().Handle(isLoading => IsLoading = isLoading);
        if (response.IsSuccess)
            AvailableProducts = mapper.Map<ObservableCollection<ProductViewModel>>(response.Data!);
        else ErrorMessage = response.Message ?? "Mahsulotlarni yuklashda xatolik.";
    }

    public async Task LoadSaleHistoryAsynce()
    {
        var response = await client.Sales.GetAll().Handle(isLoading => IsLoading = isLoading);

        if (!response.IsSuccess)
        {
            ErrorMessage = response.Message ?? "Savdo ma'lumotlarini yuklashda xatolik.";
            return;
        }


    }

}
