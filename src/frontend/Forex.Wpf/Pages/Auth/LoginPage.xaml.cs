namespace Forex.Wpf.Pages.Auth;

using Forex.ClientService;
using Forex.ClientService.Services;
using Forex.Wpf.Enums;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Services;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for LoginPage.xaml
/// </summary>
public partial class LoginPage : Page
{
    private readonly ForexClient client;
    private readonly LoginViewModel viewModel;

    public LoginPage(ForexClient client)
    {
        InitializeComponent();
        this.client = client;
        this.viewModel = new LoginViewModel(client);

        tbLogin.Focus();

        FocusNavigator.AttachEnterNavigation([
            tbLogin,
            pbPassword,
            chRemember,
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
        {
            NotificationService.Show($"{AuthStore.Instance.FullName}, Forex tizimiga muvaffaqiyatli kirildi", NotificationType.Success);
            NavigationService?.Navigate(new HomePage(client));
        }
        else
        {
            lblError.Text = viewModel.ErrorMessage;
            lblError.Visibility = Visibility.Visible;
        }
    }

    private void BtnGoRegister_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new RegisterPage(client));
    }
}



