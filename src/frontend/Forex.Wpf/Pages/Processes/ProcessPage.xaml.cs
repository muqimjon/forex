namespace Forex.Wpf.Pages.Processes;

using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Processes.ViewModels;
using Forex.Wpf.Pages.Products.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;


/// <summary>
/// Interaction logic for ProcessPage.xaml
/// </summary>
public partial class ProcessPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private ProcessPageViewModel vm;
    public ProcessPage()
    {
        InitializeComponent();
        vm = App.AppHost!.Services.GetRequiredService<ProcessPageViewModel>();
        DataContext = vm;

    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }

}
