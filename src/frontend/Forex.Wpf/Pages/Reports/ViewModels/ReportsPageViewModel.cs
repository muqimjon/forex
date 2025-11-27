namespace Forex.Wpf.Pages.Reports.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.Wpf.Common.Interfaces;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.Pages.Sales.ViewModels;
using System.Windows;
using System.Windows.Controls;


public partial class ReportsPageViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    // Har bir tab uchun alohida ViewModel
    public SalesHistoryReportViewModel SalesHistoryVM { get; }
    public FinishedStockReportViewModel FinishedStockVM { get; }
    public SemiFinishedStockReportViewModel SemiFinishedStockVM { get; }
    public DebtorCreditorReportViewModel DebtorCreditorVM { get; }
    public EmployeeBalanceReportViewModel EmployeeBalanceVM { get; }
    public CustomerSalesReportViewModel CustomerSalesVM { get; }
    public CustomerTurnoverReportViewModel CustomerTurnoverVM { get; }

    public IRelayCommand BackCommand { get; }

    public ReportsPageViewModel(
        INavigationService navigation,
        SalesHistoryReportViewModel salesHistoryVM,
        FinishedStockReportViewModel finishedStockVM,
        SemiFinishedStockReportViewModel semiFinishedStockVM,
        DebtorCreditorReportViewModel debtorCreditorVM,
        EmployeeBalanceReportViewModel employeeBalanceVM,
        CustomerSalesReportViewModel customerSalesVM,
        CustomerTurnoverReportViewModel customerTurnoverVM)
    {
        _navigation = navigation;
        SalesHistoryVM = salesHistoryVM;
        FinishedStockVM = finishedStockVM;
        SemiFinishedStockVM = semiFinishedStockVM;
        DebtorCreditorVM = debtorCreditorVM;
        EmployeeBalanceVM = employeeBalanceVM;
        CustomerSalesVM = customerSalesVM;
        CustomerTurnoverVM = customerTurnoverVM;

        // Orqaga tugmasi — Frame orqali yoki NavigationService orqali
        BackCommand = new RelayCommand(() =>
        {
            if (_navigation.CanGoBack)
                _navigation.GoBack();
        });
    }
}