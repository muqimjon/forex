namespace Forex.Wpf.Pages.Auth;

using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

public partial class RegisterPage : Page
{
    private readonly RegisterViewModel vm;

    public RegisterPage()
    {
        InitializeComponent();

        var services = App.AppHost!.Services;
        this.vm = services.GetRequiredService<RegisterViewModel>();
        DataContext = vm;

        FocusNavigator.RegisterElements([
            tbName,
            tbEmail,
            tbPhone,
            pbPassword,
            pbConfirm,
            btnRegister,
            ]);
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow is Window mainWindow)
            WindowResizer.AnimateToSize(mainWindow, 500, 550);
    }

    private async void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        lblError.Visibility = Visibility.Collapsed;

        var vm = (RegisterViewModel)DataContext;

        var success = await vm.RegisterAsync(
            tbName.Text.Trim(),
            tbEmail.Text.Trim(),
            tbPhone.Text.Trim(),
            pbPassword.Password,
            pbConfirm.Password);

        if (success)
        {
            NavigationService?.Navigate(new HomePage());
        }
        else
        {
            lblError.Text = vm.ErrorMessage;
            lblError.Visibility = Visibility.Visible;
        }
    }

    private void BtnGoLogin_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
        {
            var services = App.AppHost!.Services;
            var loginPage = services.GetRequiredService<LoginPage>();
            NavigationService?.Navigate(loginPage);
        }
    }
}
