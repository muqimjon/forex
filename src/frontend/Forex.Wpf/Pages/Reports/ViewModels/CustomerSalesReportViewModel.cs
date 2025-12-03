namespace Forex.Wpf.Pages.Sales.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.Pages.Reports.ViewModels;
using Forex.Wpf.ViewModels;
using System.Collections.ObjectModel;

// CustomerSalesReportViewModel.cs
public partial class CustomerSalesReportViewModel : ViewModelBase
{
    private readonly ForexClient _client;
    private readonly CommonReportDataService _commonData;

    //[ObservableProperty] private ObservableCollection<CustomerSalesItemViewModel> items = [];

    public ObservableCollection<UserViewModel> AvailableCustomers => _commonData.AvailableCustomers;
    [ObservableProperty] private UserViewModel? selectedCustomer;
    [ObservableProperty] private DateTime? beginDate = DateTime.Today.AddMonths(-1);
    [ObservableProperty] private DateTime? endDate = DateTime.Today;

    public CustomerSalesReportViewModel(ForexClient client, CommonReportDataService commonData)
    {
        _client = client;
        _commonData = commonData;

        PropertyChanged += (_, e) =>
        {
            if (new[] { nameof(SelectedCustomer), nameof(BeginDate), nameof(EndDate) }.Contains(e.PropertyName))
                _ = LoadAsync();
        };

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        //Items.Clear();

        //var response = await _client.Reports.GetCustomerSales(
        //    beginDate: BeginDate,
        //    endDate: EndDate,
        //    customerId: SelectedCustomer?.Id
        //).Handle(l => IsLoading = l);

        //if (!response.IsSuccess || response.Data == null)
        //{ ErrorMessage = "Mijozlar savdosi yuklanmadi"; return; }

        //foreach (var c in response.Data)
        //{
        //    Items.Add(new CustomerSalesItemViewModel
        //    {
        //        Customer = c.CustomerName,
        //        TotalSoldCount = c.TotalSoldCount,
        //        TotalAmount = c.TotalAmount
        //    });
        //}
    }
}