namespace Forex.Wpf.Pages.Transactions.Views;

using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Transactions.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class TransactionPage : Page
{
    private static readonly Regex NumericRegex = new(@"^[0-9]+$", RegexOptions.Compiled);
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;

    public TransactionPage()
    {
        InitializeComponent();
        DataContext = App.AppHost!.Services.GetRequiredService<TransactionPageViewModel>();
        SetupFocusNavigation();
    }

    private void SetupFocusNavigation()
    {
        FocusNavigator.AttachEnterNavigation(
        [
            tbKirim,
            tbChiqim
        ]);
    }

    private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !NumericRegex.IsMatch(e.Text);
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }
}