namespace Forex.Wpf.Pages.Products.ViewModels;

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

public partial class ProductPageViewModel(ForexClient Client, IMapper Mapper) : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];
    [ObservableProperty] private ObservableCollection<ProductEntryViewModel> productEntries = [];

    #region Commands

    [RelayCommand]
    private void DeleteProduct(ProductEntryViewModel? entry)
    {
        if (entry is null) return;

        ProductEntries.Remove(entry);

        if (ProductEntries.Count == 0)
            ProductEntries = [];
    }

    [RelayCommand]
    private async Task Submit()
    {
        if (!ProductEntries.Any())
        {
            WarningMessage = "Mahsulotlar ro'yxati bo'sh!";
            return;
        }

        var requests = ProductEntries
            .Where(p => p.Product is not null && p.ProductType is not null && p.Count > 0)
            .Select(p => new ProductEntryRequest
            {
                Count = p.Count,
                BundleItemCount = (uint)p.ProductType!.BundleItemCount!
            })
            .ToList();

        if (requests.Count == 0)
        {
            WarningMessage = "Hech qanday to'g'ri mahsulot kiritilmagan!";
            return;
        }

        var response = await Client.ProductEntries.Create(new() { Command = requests })
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            SuccessMessage = "Mahsulotlar muvaffaqiyatli saqlandi.";
            ProductEntries.Clear();
        }
        else
        {
            ErrorMessage = response.Message ?? "Mahsulotlarni saqlashda xatolik yuz berdi.";
        }
    }

    #endregion Commands

    #region Load Data

    public async Task InitializeAsync()
    {
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["ProductType"] = ["include:product"],
                ["bundleItemCount"] = [">1"]
            }
        };

        var response = await Client.Processes.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (!response.IsSuccess)
        {
            ErrorMessage = response.Message ?? "Mahsulotlarni yuklashda xatolik.";
            return;
        }

        var inProcesses = Mapper.Map<List<InProcessViewModel>>(response.Data);

        //inProcesses.ForEach(p => p.ProductType.AvailableCount = p.Count);

        var allTypes = inProcesses
           .Select(p => p.ProductType)
           .Where(pt => pt is not null && pt.Product is not null)
           .ToList();

        var grouped = allTypes
            .GroupBy(pt => pt.Product.Id);

        var products = new ObservableCollection<ProductViewModel>();

        foreach (var group in grouped)
        {
            var sampleType = group.First();
            var product = sampleType.Product;

            product.ProductTypes = new ObservableCollection<ProductTypeViewModel>(group);

            products.Add(product);
        }

        AvailableProducts = products;
    }

    #endregion Load Data
}