namespace Forex.Wpf.Pages.Sales.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Spreadsheet;
using global::Forex.ClientService;
using global::Forex.Wpf.Pages.Common;
using System.Collections.ObjectModel;

// EmployeeBalanceReportViewModel.cs
public partial class EmployeeBalanceReportViewModel : ViewModelBase
{
    private readonly ForexClient _client;

    //[ObservableProperty] private ObservableCollection<EmployeeBalanceItemViewModel> items = [];

    public EmployeeBalanceReportViewModel(ForexClient client)
    {
        _client = client;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        //Items.Clear();

        //var response = await _client.Employees.GetBalances().Handle(l => IsLoading = l);
        //if (!response.IsSuccess || response.Data == null)
        //{ ErrorMessage = "Hodimlar balansi yuklanmadi"; return; }

        //foreach (var e in response.Data)
        //{
        //    Items.Add(new EmployeeBalanceItemViewModel
        //    {
        //        FullName = e.FullName,
        //        Balance = e.Balance,
        //        LastPaymentDate = e.LastPaymentDate
        //    });
        //}
    }
}