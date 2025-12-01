namespace Forex.Wpf.Pages.Sales.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using Forex.ClientService;
using Forex.ClientService.Extensions;
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
        //Items.Clear();

        //var response = await _client.Users.GetAllAsync().Handle(l => IsLoading = l);
        //if (!response.IsSuccess)
        //{
        //    ErrorMessage = "Debitor/Kreditor ma'lumotlari yuklanmadi";
        //    return;
        //}

        //foreach (var account in response.Data)
        //{
        //    var user = account.User;

        //    if (SelectedCustomer != null && user.Id != SelectedCustomer.Id)
        //        continue;

        //    decimal balance = account.Balance;

        //    Items.Add(new DebtorCreditorItemViewModel
        //    {
        //        Id = account.Id,
        //        Name = user.Name,
        //        Phone = user.Phone,
        //        Address = user.Address,

        //        DebtorAmount = balance < 0 ? Math.Abs(balance) : 0,  // qarzdorlik (bizga qarzi)
        //        CreditorAmount = balance > 0 ? balance : 0           // bizning qarz
        //    });
        //}
    }

}