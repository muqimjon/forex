namespace Forex.Wpf.Pages.Auth;

using Forex.ClientService;
using Forex.ClientService.Services;
using Forex.Wpf.Enums;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Services;
using Forex.Wpf.ViewModels;
using System.Windows;
using System.Windows.Controls;

public partial class RegisterPage : Page
{
    private readonly ForexClient client;
    private readonly RegisterViewModel vm;

    public RegisterPage(ForexClient client)
    {
        InitializeComponent();
        this.client = client;
        this.vm = new RegisterViewModel(client);
        DataContext = vm;
        tbName.Focus();

        FocusNavigator.AttachEnterNavigation([
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
            NotificationService.Show($"{AuthStore.Instance.FullName} Forex tizimiga hush kelibsiz", NotificationType.Success);
            NavigationService?.Navigate(new HomePage(client));
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
            NavigationService?.Navigate(new LoginPage(client));
    }
}
