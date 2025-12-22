namespace Forex.Wpf.Pages.Home;

using Forex.ClientService;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Services;
using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Auth;
using Forex.Wpf.Pages.Processes;
using Forex.Wpf.Pages.Products;
using Forex.Wpf.Pages.Reports;
using Forex.Wpf.Pages.Sales;
using Forex.Wpf.Pages.SemiProducts.Views;
using Forex.Wpf.Pages.Settings;
using Forex.Wpf.Pages.Transactions.Views;
using Forex.Wpf.Pages.Users;
using Forex.Wpf.Windows;
using Forex.Wpf.Windows.OverdueAccountsWindow;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

/// <summary>
/// Interaction logic for HomePage.xaml
/// </summary>
public partial class HomePage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private readonly ForexClient client = App.AppHost!.Services.GetRequiredService<ForexClient>();

    public HomePage()
    {
        InitializeComponent();
        DataContext = AuthStore.Instance;

        Loaded += Page_Loaded;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        this.ResizeWindow(810, 580);
        RegisterFocusNavigation();
        RegisterGlobalShortcuts();

        //var userAccountService = App.AppHost!.Services.GetRequiredService<IApiUserAccount>();
        //var allAccountsResult = await userAccountService.GetAllAsync();

        //if (allAccountsResult.Data == null)
        //{
        //    if (Application.Current.MainWindow is Window defaultWindow)
        //        defaultWindow.Background = Brushes.Green;
        //    return;
        //}

        //var filteredData = allAccountsResult.Data
        //   //.Where(a =>
        //   //    a.DueDate.HasValue &&
        //   //    a.DueDate.Value.Date <= DateTime.Today)
        //   .ToList();

        //if (filteredData.Count > 0)
        //{
        //    tbWarning.Foreground = Brushes.Red;
        //}
        //else
        //{
        //    tbWarning.Foreground = Brushes.Green;
        //}
    }

    private void RegisterGlobalShortcuts()
    {
        btnSale.RegisterShortcut(Key.F1);
        btnCash.RegisterShortcut(Key.F2);
        btnProduct.RegisterShortcut(Key.F3);
        btnSemiProductEntry.RegisterShortcut(Key.F4);
        btnUser.RegisterShortcut(Key.F5);
        btnReports.RegisterShortcut(Key.F6);
        btnSettings.RegisterShortcut(Key.F7);
    }

    private void RegisterFocusNavigation()
    {
        FocusNavigator.RegisterElements(
        [
            btnSale,
            btnCash,
            btnProduct,
            btnSemiProductEntry,
            btnUser,
            btnReports,
            btnSettings,
        ]);
    }

    private void BtnUser_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new UserPage());

    private void BtnProduct_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new ProductPage());

    private void BtnCash_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new TransactionPage());

    private void BtnSale_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new SalePage());

    private void BtnSettings_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new SettingsPage());

    private void BtnSemiProduct_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new SemiProductPage());

    private void btnReports_Click(object sender, RoutedEventArgs e)
        => Main.NavigateTo(new ReportsPage());

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        AuthStore.Instance.Logout();
        Main.NavigateTo(new LoginPage());
    }

    private void btnProcess_Click(object sender, RoutedEventArgs e)
          => Main.NavigateTo(new ProcessPage());

    private void BtnOverdue_Click(object sender, RoutedEventArgs e)
    {
        var window = new OverdueAccountsWindow();
        window.ShowDialog();
    }


    // Profilni bosganda menyuni chiqarish
    private void BtnProfile_Click(object sender, RoutedEventArgs e)
    {
        if (pnlProfileMenu.Visibility == Visibility.Collapsed)
        {
            pnlProfileMenu.Visibility = Visibility.Visible;

            // Oddiygina paydo bo'lish animatsiyasi
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.2));
            pnlProfileMenu.BeginAnimation(OpacityProperty, fadeIn);
        }
        else
        {
            pnlProfileMenu.Visibility = Visibility.Collapsed;
        }
    }

    // 3 ta nuqta bosilganda rasm tanlash logic
    private void BtnChangePhoto_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Rasm fayllari (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            imgProfile.ImageSource = new BitmapImage(new Uri(openFileDialog.FileName));
            // Bu yerda rasmni serverga yuborish kodingiz bo'ladi
        }
    }

    // OK bosilganda parolni saqlash
    private async void BtnSavePassword_Click(object sender, RoutedEventArgs e)
    {
        var newPassword = txtNewPassword.Password;

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 4)
        {
            MessageBox.Show("Parol juda qisqa!", "Xato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        pnlProfileMenu.IsEnabled = false;

        try
        {
            // 1. Foydalanuvchi ma'lumotlarini to'liq olib kelamiz
            var userResponse = await client.Users.GetById(AuthStore.Instance.UserId);

            if (userResponse?.Data != null)
            {
                var u = userResponse.Data;

                // 2. UserRequest obyektini server talab qilganidek to'liq to'ldiramiz
                var updateRequest = new UserRequest
                {
                    Id = u.Id,
                    Name = u.Name, // Modelda Name majburiy bo'lishi mumkin
                    //UserName = u.UserName,
                    Phone = u.Phone,
                    Email = u.Email,
                    Role = u.Role,
                    Address = u.Address,
                    Description = u.Description,
                    Password = newPassword, // Yangi parol

                    // MUHIM: Server Accounts'ni talab qilgani uchun eskilarini qaytarib jo'natamiz
                    Accounts = u.Accounts?.Select(a => new UserAccount
                    {
                    }).ToList() ?? new List<UserAccount>()
                };

                // 3. Update so'rovi
                var result = await client.Users.Update(updateRequest);

                if (result.Data)
                {
                    MessageBox.Show("Parol muvaffaqiyatli yangilandi!", "Muvaffaqiyat");
                    txtNewPassword.Password = "";
                    pnlProfileMenu.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MessageBox.Show($"Xatolik: {result.Message}", "Xato");
                }
            }
        }
        catch (Refit.ApiException ex)
        {
            // Agar hali ham xato bersa, serverdan kelgan aniq JSON xatolikni ko'ramiz
            var errorContent = ex.Content;
            MessageBox.Show($"Server rad etdi:\n{errorContent}", "Validatsiya xatosi");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Tizim xatosi: {ex.Message}");
        }
        finally
        {
            pnlProfileMenu.IsEnabled = true;
        }
    }
}
