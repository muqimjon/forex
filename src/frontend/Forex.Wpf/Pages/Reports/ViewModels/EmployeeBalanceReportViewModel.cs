namespace Forex.Wpf.Pages.Reports.ViewModels;

using Forex.ClientService;
using Forex.Wpf.Pages.Common;

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