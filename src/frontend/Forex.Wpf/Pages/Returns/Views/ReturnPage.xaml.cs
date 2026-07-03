namespace Forex.Wpf.Pages.Returns.Views;

using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Returns.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class ReturnPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private readonly ReturnPageViewModel vm;

    public ReturnPage()
    {
        InitializeComponent();
        vm = App.AppHost!.Services.GetRequiredService<ReturnPageViewModel>();
        DataContext = vm;

        Loaded += Page_Loaded;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        btnBack.RegisterShortcut(Key.Escape);
        btnAdd.RegisterShortcut(Key.Add);
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }

    private void BtnAddReturn_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new AddReturnPage());
}
