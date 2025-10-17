namespace Forex.Wpf.Pages.Auth;

using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for LoginPage.xaml
/// </summary>
public partial class LoginPage : Page
{
    private readonly LoginViewModel viewModel;

    public LoginPage()
    {
        InitializeComponent();
        var services = App.AppHost!.Services;
        viewModel = services.GetRequiredService<LoginViewModel>();
        DataContext = viewModel;

        tbLogin.Focus();

        FocusNavigator.AttachEnterNavigation([
            tbLogin,
            pbPassword,
            btnLogin,
            ]);
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow is Window mainWindow)
            WindowResizer.AnimateToSize(mainWindow, 500, 450);
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        lblError.Visibility = Visibility.Collapsed;

        var success = await viewModel.LoginAsync(tbLogin.Text.Trim(), pbPassword.Password);

        if (success)
            NavigationService?.Navigate(new HomePage());
        else
        {
            lblError.Text = viewModel.ErrorMessage;
            lblError.Visibility = Visibility.Visible;
        }
    }

    private void BtnGoRegister_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new RegisterPage());
    }
}



