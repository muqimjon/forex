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

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        this.ResizeWindow(1080, 700);
        RegisterFocusNavigation();
        RegisterGlobalShortcuts();
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

        this.RegisterShortcut(Key.F1, () => tabReports.SelectedIndex = 0);
        this.RegisterShortcut(Key.F2, () => tabReports.SelectedIndex = 1);
        this.RegisterShortcut(Key.F3, () => tabReports.SelectedIndex = 2);
        this.RegisterShortcut(Key.F4, () => tabReports.SelectedIndex = 3);
        this.RegisterShortcut(Key.F5, () => tabReports.SelectedIndex = 4);
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }
}
