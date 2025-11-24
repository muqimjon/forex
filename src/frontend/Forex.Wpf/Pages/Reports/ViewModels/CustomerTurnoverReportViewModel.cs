namespace Forex.Wpf.Pages.Sales.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using global::Forex.ClientService;
using global::Forex.Wpf.Pages.Common;
using global::Forex.Wpf.Pages.Reports.ViewModels;
using global::Forex.Wpf.ViewModels;
using System.Collections.ObjectModel;

public partial class CustomerTurnoverReportViewModel : ViewModelBase
{
    private readonly ForexClient _client;
    private readonly CommonReportDataService _commonData;

    // Filtrlar
    [ObservableProperty] private UserViewModel? _selectedCustomer;
    [ObservableProperty] private DateTime? _beginDate = DateTime.Today.AddDays(-30);
    [ObservableProperty] private DateTime? _endDate = DateTime.Today;

    // Ma'lumotlar
    public ObservableCollection<UserViewModel> AvailableCustomers => _commonData.AvailableCustomers;
    public ObservableCollection<TurnoversViewModel> Operations { get; } = new();

    // Qoldiqlar
    [ObservableProperty] private decimal _beginBalance;
    [ObservableProperty] private decimal _lastBalance;

    public CustomerTurnoverReportViewModel(ForexClient client, CommonReportDataService commonData)
    {
        _client = client;
        _commonData = commonData;

        // Filtr o‘zgarsa → avto yangilash
        this.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName is nameof(SelectedCustomer) or nameof(BeginDate) or nameof(EndDate))
                await LoadAsync();
        };

        // Dastlabki yuklash
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        Operations.Clear();

        try
        {
            var begin = BeginDate?.Date ?? DateTime.Today.AddDays(-30);
            var end = (EndDate?.Date ?? DateTime.Today).AddDays(1);

            var request = new FilteringRequest
            {
                Filters = new()
                {
                    ["date"] = new() { $">={begin:yyyy-MM-dd}", $"<{end:yyyy-MM-dd}" },
                    ["include"] = new() { "customer", "payments", "sales" }
                }
            };

            if (SelectedCustomer != null)
                request.Filters["customerId"] = new() { SelectedCustomer.Id.ToString() };

            var response = await _client.Transactions.Filter(request).Handle(l => IsLoading = l);

            if (!response.IsSuccess || response.Data == null)
            {
                ErrorMessage = "Ma'lumotlar yuklanmadi";
                return;
            }

            var operations = response.Data;

            // Boshlang‘ich qoldiq (davrdan oldingi qoldiq)
            //BeginBalance = operations.Any() ? operations.First() : 0m;
            //LastBalance = operations.Any() ? operations.Last() : BeginBalance;

            decimal currentBalance = BeginBalance;

            foreach (var op in operations)
            {

                Operations.Add(new TurnoversViewModel
                {
                    Date = op.Date,
                    Debit = op.Amount,
                    Credit = op.Amount,
                    Description = ""
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
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
        BeginDate = DateTime.Today.AddDays(-30);
        EndDate = DateTime.Today;
    }

    // Keyinroq qo‘shiladi
    [RelayCommand] private void Preview() => ShowNotImplemented();
    [RelayCommand] private void Print() => ShowNotImplemented();
    [RelayCommand] private async Task ExportToExcel() => await Task.CompletedTask;

    private void ShowNotImplemented() =>
        System.Windows.MessageBox.Show("Tez orada qo‘shiladi!", "Info",
            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
}
