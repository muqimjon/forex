using Forex.Wpf.ViewModels;

namespace Forex.Wpf.Pages.Reports.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

public partial class CustomerSalesRatingViewModel : ViewModelBase
{
    private readonly ForexClient _client;
    private readonly CommonReportDataService _commonData;

    [ObservableProperty] private ObservableCollection<CustomerSaleViewModel> customerSales = [];
    public ObservableCollection<UserViewModel> AvailableCustomers => _commonData.AvailableCustomers;

    [ObservableProperty] private UserViewModel? selectedCustomer;
    [ObservableProperty] private DateTime beginDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    [ObservableProperty] private DateTime endDate = DateTime.Today;

    public CustomerSalesRatingViewModel(ForexClient client, CommonReportDataService commonData)
    {
        _client = client;
        _commonData = commonData;
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(BeginDate) or nameof(EndDate) or nameof(SelectedCustomer))
                LoadSalesAsync();
        };
        LoadSalesAsync();
    }

    private async void LoadSalesAsync()
    {
        IsLoading = true;

        try
        {
            var request = new FilteringRequest
            {
                Filters = new()
                {
                    ["date"] =
                [
                    $">={BeginDate:dd.MM.yyyy}",
                    $"<{EndDate.AddDays(1):dd.MM.yyyy}"
                ],
                    ["customer"] = ["include"],
                    ["saleItems"] = ["include:productType.product"]
                }

            };
            // ... filter va request ...

            var response = await _client.Sales.Filter(request).Handle(l => IsLoading = l);
            if (!response.IsSuccess || response.Data == null)
            {
                ErrorMessage = "Savdolar yuklanmadi";
                CustomerSales = new ObservableCollection<CustomerSaleViewModel>(); // yangi collection
                return;
            }

            var tempList = new List<CustomerSaleViewModel>();
            int rowNumber = 1;

            foreach (var group in response.Data
                .Where(s => s.Customer != null)
                .GroupBy(s => s.Customer.Id))
            {
                var customer = group.First().Customer;

                if (SelectedCustomer != null && SelectedCustomer.Id != customer.Id)
                    continue;

                var vm = new CustomerSaleViewModel
                {
                    RowNumber = rowNumber++,
                    CustomerName = customer.Name ?? "Nomsiz mijoz"
                };

                foreach (var sale in group)
                {
                    if (sale.SaleItems == null) continue;

                    foreach (var item in sale.SaleItems)
                    {
                        if (item?.ProductType?.Product == null) continue;

                        var origin = item.ProductType.Product.ProductionOrigin;
                        int count = (int)item.TotalCount;

                        switch (origin)
                        {
                            case ProductionOrigin.Tayyor: vm.ReadyCount += count; break;
                            case ProductionOrigin.Aralash: vm.MixedCount += count; break;
                            case ProductionOrigin.Eva: vm.EvaCount += count; break;
                            default: vm.ReadyCount += count; break;
                        }
                    }
                }

                tempList.Add(vm);
            }

            // Eng muhimi — YANGI ObservableCollection yaratib beramiz!
            CustomerSales = new ObservableCollection<CustomerSaleViewModel>(tempList);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}");
            CustomerSales = new ObservableCollection<CustomerSaleViewModel>();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ClearFilter()
    {
        SelectedCustomer = null;
        BeginDate = DateTime.Today.AddMonths(-1);
        EndDate = DateTime.Today;
        LoadSalesAsync();
    }

    [RelayCommand]
    private void ExportToExcel()
    {
        // Excel export code here
    }

    [RelayCommand]
    private void Print()
    {
        // Print code here
    }

    [RelayCommand]
    private void Preview()
    {
        // Preview code here
    }
}