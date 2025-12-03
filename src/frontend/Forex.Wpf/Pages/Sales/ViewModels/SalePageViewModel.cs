namespace Forex.Wpf.Pages.Sales.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.Wpf.Common.Interfaces;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;

public partial class SalePageViewModel : ViewModelBase, INavigationAware
{
    private readonly ForexClient client = App.AppHost!.Services.GetRequiredService<ForexClient>();
    private readonly IMapper mapper = App.AppHost!.Services.GetRequiredService<IMapper>();

    public SalePageViewModel()
    {
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(BeginDate) or nameof(EndDate))
                _ = LoadSalesAsync();
        };

        _ = LoadDataAsync();
    }

    [ObservableProperty] private ObservableCollection<SaleViewModel> sales = [];
    [ObservableProperty] private ObservableCollection<UserViewModel> availableCustomers = [];
    [ObservableProperty] private SaleViewModel? selectedSale;

    [ObservableProperty] private DateTime beginDate = DateTime.Today;
    [ObservableProperty] private DateTime endDate = DateTime.Today;
    [ObservableProperty] private UserViewModel? selectedCustomer;

    #region Load Data

    private async Task LoadDataAsync()
    {
        await Task.WhenAll(
            LoadCustomersAsync(),
            LoadSalesAsync()
            );
    }

    private async Task LoadCustomersAsync()
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["role"] = ["mijoz"]
            }
        };

        var response = await client.Users.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            AvailableCustomers = mapper.Map<ObservableCollection<UserViewModel>>(response.Data);
        }
        else
        {
            ErrorMessage = response.Message ?? "Mijozlarni yuklashda xatolik.";
        }
    }

    private async Task LoadSalesAsync()
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["date"] =
                [
                    $">={BeginDate:dd-MM-yyyy}",
                    $"<{EndDate.AddDays(1):dd-MM-yyyy}"
                ],
                ["customer"] = ["include"],
                ["saleItems"] = ["include:productType.product"]
            }
        };

        // Agar mijoz tanlangan bo'lsa, filter qo'shamiz
        if (SelectedCustomer is not null)
        {
            request.Filters["customerId"] = [SelectedCustomer.Id.ToString()];
        }

        var response = await client.Sales.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            var ordered = response.Data.OrderByDescending(s => s.Date).ToList();
            Sales = mapper.Map<ObservableCollection<SaleViewModel>>(ordered);
        }
        else
        {
            WarningMessage = response.Message ?? "Savdolarni yuklashda xatolik.";
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task FilterSales()
    {
        await LoadSalesAsync();
    }

    [RelayCommand]
    private async Task Delete(SaleViewModel value)
    {
        if (value is null)
            return;

        var result = MessageBox.Show(
            $"Savdoni o'chirishni tasdiqlaysizmi?\n\nMijoz: {value.Customer?.Name}\nSana: {value.Date:dd.MM.yyyy}\nSumma: {value.TotalAmount:N2} so'm\nMahsulotlar soni: {value.SaleItems?.Count ?? 0}",
            "O'chirishni tasdiqlash",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.No)
            return;

        var response = await client.Sales.Delete(value.Id)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            Sales.Remove(value);
            SuccessMessage = "Savdo muvaffaqiyatli o'chirildi";
            await LoadSalesAsync();
        }
        else
        {
            ErrorMessage = response.Message ?? "Savdoni o'chirishda xatolik";
        }
    }

    #endregion

    #region Property Changes

    partial void OnSelectedCustomerChanged(UserViewModel? value)
    {
        _ = LoadSalesAsync();
    }

    #endregion

    #region Private Helpers

    public void OnNavigatedTo()
    {
        _ = LoadDataAsync();
    }

    public void OnNavigatedFrom()
    {
    }

    #endregion Private Helpers
}