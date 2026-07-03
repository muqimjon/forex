namespace Forex.Wpf.Pages.Reports;

using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Reports.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class ReportsPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;

    public ReportsPage()
    {
        InitializeComponent();

        DataContext = App.AppHost!.Services.GetRequiredService<ReportsPageViewModel>();

        Loaded += Page_Loaded;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        this.ResizeWindow(1080, 700);
        RegisterFocusNavigation();
        RegisterGlobalShortcuts();

        await App.AppHost!.Services.GetRequiredService<CommonReportDataService>().RefreshAsync();
    }

    private void RegisterFocusNavigation()
    {
        FocusNavigator.RegisterElements([
            tabReports
            ]);
    }

    private void RegisterGlobalShortcuts()
    {
        btnBack.RegisterShortcut(Key.Escape);
        tabReports.RegisterTabShortcuts();
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }
}
