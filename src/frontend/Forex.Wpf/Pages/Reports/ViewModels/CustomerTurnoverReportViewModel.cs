namespace Forex.Wpf.Pages.Reports.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using global::Forex.Wpf.Pages.Common;
using global::Forex.Wpf.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public partial class CustomerTurnoverReportViewModel : ViewModelBase
{
    private readonly ForexClient _client;
    private readonly CommonReportDataService _commonData;

    [ObservableProperty] private UserViewModel? _selectedCustomer;
    [ObservableProperty] private DateTime? _beginDate = DateTime.Today.AddMonths(-1);
    [ObservableProperty] private DateTime? _endDate = DateTime.Today;

    public ObservableCollection<UserViewModel> AvailableCustomers => _commonData.AvailableCustomers;
    public ObservableCollection<TurnoversViewModel> Operations { get; } = new();

    [ObservableProperty] private decimal _beginBalance;
    [ObservableProperty] private decimal _lastBalance;

    public CustomerTurnoverReportViewModel(ForexClient client, CommonReportDataService commonData)
    {
        _client = client;
        _commonData = commonData;

        this.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName is nameof(SelectedCustomer) or nameof(BeginDate) or nameof(EndDate))
                await LoadDataAsync();
        };

        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (SelectedCustomer == null) { Operations.Clear(); return; }

        Operations.Clear();

        var begin = BeginDate ?? DateTime.Today.AddMonths(-1);
        var end = EndDate ?? DateTime.Today;

        var request = new FilteringRequest
        {
            Filters = new()
            {
                ["date"] = [$">={begin:yyyy-MM-dd}", $"<{end.AddDays(1):yyyy-MM-dd}"]
            }
        };

        var response = await _client.Transactions.Filter(request).Handle(l => IsLoading = l);

        if (response.IsSuccess)
        {
            //BeginBalance = response.Data.FirstOrDefault()?.BeginBalance ?? 0;
            //decimal balance = BeginBalance;

            //foreach (var op in response.Data)
            //{
            //    balance += op.Debit - op.Credit;
            //    Operations.Add(new TurnoversViewModel
            //    {
            //        Date = op.Date,
            //        Description = op.Description ?? "",
            //        Debit = op.Debit,
            //        Credit = op.Credit
            //    });
            //}
            //LastBalance = balance;
        }

    }

    [RelayCommand] private void ClearFilter() => SelectedCustomer = null;
    //[RelayCommand] private void Preview() => ShowNotImplemented();
    //[RelayCommand] private void Print() => ShowNotImplemented();
    [RelayCommand] private async Task ExportToExcel() => await Task.CompletedTask;
}
