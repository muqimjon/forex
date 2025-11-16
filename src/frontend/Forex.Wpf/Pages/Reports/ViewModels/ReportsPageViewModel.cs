namespace Forex.Wpf.Pages.Reports.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.Wpf.Pages.Common;
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
        await LoadSaleAsynce();
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

    //public async Task LoadSaleAsynce()
    //{
    //    FilteringRequest request = new()
    //    {
    //        Filters = new()
    //        {
    //            ["SaleItems"] = ["include:productType.product"],
    //            ["SaleItems"] = ["include:productType.ProductTypeItems"],
    //        }
    //    };

    //    var response = await client.Sales.Filter(request).Handle(isLoading => IsLoading = isLoading);
    //    if (!response.IsSuccess)
    //    {
    //        ErrorMessage = response.Message ?? "Savdo ma'lumotlarini yuklashda xatolik.";
    //        return;
    //    }
    //}

    public async Task LoadSaleAsynce()
    {
        //FilteringRequest request = new()
        //{
        //    Filters = new()
        //    {
        //        ["SaleItems"] = ["include:productType.product"],
        //        ["SaleItems"] = ["include:productType.ProductTypeItems"],
        //        ["Customer"] = ["include:"]
        //    }
        //};

        var response = await client.Sales.GetAll().Handle(isLoading => IsLoading = isLoading);

        if (!response.IsSuccess)
        {
            ErrorMessage = response.Message ?? "Savdo ma'lumotlarini yuklashda xatolik.";
            return;
        }

        var items = new ObservableCollection<SaleItemForReportViewModel>();

        foreach (var sale in response.Data)
        {
            foreach (var item in sale.SaleItems)
            {
                items.Add(new SaleItemForReportViewModel
                {
                    Date = sale.Date,
                    Customer = sale.Customer?.Name,
                    BundleCount = item.ProductType.BundleItemCount,
                    TotalCount = item.TotalCount,
                    Amount = item.TotalAmount
                });
            }
        }

        SaleItems = items;
        FilteredSaleItems = items; // agar filtirdan foydalanmoqchi bo‘lsangiz
    }

}
