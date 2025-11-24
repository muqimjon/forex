namespace Forex.Wpf.Pages.Sales.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Spreadsheet;
using global::Forex.ClientService;
using global::Forex.Wpf.Pages.Common;
using global::Forex.Wpf.Pages.Reports.ViewModels;
using global::Forex.Wpf.ViewModels;
using System.Collections.ObjectModel;

// DebtorCreditorReportViewModel.cs
public partial class DebtorCreditorReportViewModel : ViewModelBase
{
    private readonly ForexClient _client;
    private readonly CommonReportDataService _commonData;

    //[ObservableProperty] private ObservableCollection<DebtorCreditorItemViewModel> items = [];

    public ObservableCollection<UserViewModel> AvailableCustomers => _commonData.AvailableCustomers;
    [ObservableProperty] private UserViewModel? selectedCustomer;

    public DebtorCreditorReportViewModel(ForexClient client, CommonReportDataService commonData)
    {
        _client = client;
        _commonData = commonData;

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SelectedCustomer))
                _ = LoadAsync();
        };

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        // Items.Clear();

        ////var response = await _client.Reports.GetDebtorCreditor().Handle(l => IsLoading = l);
        ////if (!response.IsSuccess || response.Data == null)
        ////{ ErrorMessage = "Debitor/Kreditor ma'lumotlari yuklanmadi"; return; }

        ////foreach (var item in response.Data)
        ////{
        ////    if (SelectedCustomer != null && item.CustomerId != SelectedCustomer.Id) continue;

        ////    Items.Add(new DebtorCreditorItemViewModel
        ////    {
        ////        Customer = item.CustomerName,
        ////        Debitor = item.Debitor,
        ////        Creditor = item.Creditor,
        ////        Balance = item.Balance
        ////    });
        //}
    }
}