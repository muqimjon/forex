namespace Forex.Wpf.Pages.Sales.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.Pages.Reports.ViewModels;
using Forex.Wpf.ViewModels;
using System.Collections.ObjectModel;

// DebtorCreditorReportViewModel.cs
public partial class DebtorCreditorReportViewModel : ViewModelBase
{
    private readonly ForexClient _client;
    private readonly CommonReportDataService _commonData;

    [ObservableProperty] private ObservableCollection<DebtorCreditorItemViewModel> items = [];

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
        Items.Clear();

        var users = await LoadUsersAsync();
        if (users == null) return;

        var mapped = MapUsersToDebtorCreditor(users);

        foreach (var item in mapped)
            Items.Add(item);
    }

    private async Task<List<UserResponse>?> LoadUsersAsync()
    {
        var response = await _client.Users.GetAllAsync().Handle(l => IsLoading = l);

        if (!response.IsSuccess)
        {
            ErrorMessage = "Foydalanuvchilar yuklanmadi";
            return null;
        }

        if (SelectedCustomer != null)
            return response.Data.Where(u => u.Id == SelectedCustomer.Id).ToList();

        return response.Data.ToList();
    }

    private List<DebtorCreditorItemViewModel> MapUsersToDebtorCreditor(
    List<UserResponse> users)
    {
        var list = new List<DebtorCreditorItemViewModel>();

        foreach (var u in users)
        {
            var balance = u.FirstBalance ?? 0;

            list.Add(new DebtorCreditorItemViewModel
            {
                Id = u.Id,               // account emas, user ID
                Name = u.Name,
                Phone = u.Phone,
                Address = u.Address,

                DebtorAmount = balance < 0 ? Math.Abs(balance) : 0,  // qarzdor
                CreditorAmount = balance > 0 ? balance : 0           // kreditor
            });
        }

        return list;
    }

}