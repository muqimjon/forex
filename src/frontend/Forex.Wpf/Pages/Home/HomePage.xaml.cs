namespace Forex.Wpf.Pages.Home;

using Forex.ClientService;
using Forex.ClientService.Services;
using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Auth;
using Forex.Wpf.Pages.Barcode.Views;
using Forex.Wpf.Pages.Products;
using Forex.Wpf.Pages.Reports;
using Forex.Wpf.Pages.Returns.Views;
using Forex.Wpf.Pages.Sales;
using Forex.Wpf.Pages.Settings;
using Forex.Wpf.Pages.Supply.Views;
using Forex.Wpf.Pages.Transactions.Views;
using Forex.Wpf.Pages.Users;
using Forex.Wpf.Windows;
using Forex.Wpf.Windows.OverdueAccountsWindow;
using Forex.Wpf.Windows.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

public partial class HomePage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private readonly ForexClient client = App.AppHost!.Services.GetRequiredService<ForexClient>();

    private static readonly Brush ChipActiveBg = new SolidColorBrush(Colors.White);
    private static readonly Brush ChipActiveFg = new SolidColorBrush(Color.FromRgb(0x3B, 0x5B, 0xDB));
    private static readonly Brush ChipInactiveFg = new SolidColorBrush(Color.FromRgb(0x7A, 0x86, 0x99));

    public ProfileEditViewModel ProfileViewModel { get; }
    public HomeDashboardViewModel DashboardViewModel { get; }

    public HomePage()
    {
        InitializeComponent();
        ProfileViewModel = new ProfileEditViewModel(client);
        DashboardViewModel = new HomeDashboardViewModel(client);

        // Main content needs AuthStore for navbar bindings
        mainContent.DataContext = AuthStore.Instance;

        // Dashboard widgets bind to the dashboard view model
        dashboardRoot.DataContext = DashboardViewModel;

        // Profile modal needs ProfileViewModel
        profileEditOverlay.DataContext = ProfileViewModel;

        Loaded += Page_Loaded;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        this.ResizeWindow(1180, 800);
        RegisterFocusNavigation();
        RegisterGlobalShortcuts();
        await LoadUserAvatar();

        // Boshqaruv paneli pul/savdo aylanmasini ko'rsatadi — faqat ruxsati bo'lganlarga.
        if (AuthStore.Instance.CanDashboard)
        {
            UpdateChips("Week");
            await DashboardViewModel.LoadAsync();
        }
        else
        {
            dashboardRoot.Visibility = Visibility.Collapsed;
        }
    }

    private async void Chip_Click(object sender, RoutedEventArgs e)
    {
        var period = (string)((Button)sender).Tag;
        UpdateChips(period);
        await DashboardViewModel.LoadAsync(period);
    }

    private void UpdateChips(string period)
    {
        SetChip(chipDay, period == "Day");
        SetChip(chipWeek, period == "Week");
        SetChip(chipMonth, period == "Month");
    }

    private static void SetChip(Button button, bool active)
    {
        button.Background = active ? ChipActiveBg : Brushes.Transparent;
        button.Foreground = active ? ChipActiveFg : ChipInactiveFg;
    }

    private void RegisterGlobalShortcuts()
    {
        // Tezkor tugmalar faqat ruxsat berilgan bo'limlar uchun ishlaydi
        // (yashirin bo'limni F-tugma orqali ochib bo'lmasin).
        var auth = AuthStore.Instance;
        if (auth.CanSales) btnSale.RegisterShortcut(Key.F1);
        if (auth.CanPayments) btnCash.RegisterShortcut(Key.F2);
        if (auth.CanProducts) btnProduct.RegisterShortcut(Key.F3);
        if (auth.CanSupply) btnSupply.RegisterShortcut(Key.F4);
        if (auth.CanUsers) btnUser.RegisterShortcut(Key.F5);
        if (auth.CanReports) btnReports.RegisterShortcut(Key.F6);
        if (auth.CanSettings) btnSettings.RegisterShortcut(Key.F7);
    }

    private void RegisterFocusNavigation()
    {
        // Faqat ko'rinadigan (ruxsatli) tugmalarni fokus navigatsiyasiga qo'shamiz.
        var auth = AuthStore.Instance;
        var elements = new List<System.Windows.UIElement>();
        if (auth.CanSales) elements.Add(btnSale);
        if (auth.CanPayments) elements.Add(btnCash);
        if (auth.CanProducts) elements.Add(btnProduct);
        if (auth.CanSupply) elements.Add(btnSupply);
        if (auth.CanUsers) elements.Add(btnUser);
        if (auth.CanReports) elements.Add(btnReports);
        if (auth.CanSettings) elements.Add(btnSettings);

        FocusNavigator.RegisterElements(elements);
    }

    // Har bir handler — mudofaa chuqurligi uchun ruxsatni qayta tekshiradi
    // (tugma yashirin bo'lsa ham xavfsiz).
    private void BtnUser_Click(object sender, RoutedEventArgs e)
    {
        if (AuthStore.Instance.CanUsers) Main.NavigateTo(new UserPage());
    }

    private void BtnProduct_Click(object sender, RoutedEventArgs e)
    {
        if (AuthStore.Instance.CanProducts) Main.NavigateTo(new ProductPage());
    }

    private void BtnBarcode_Click(object sender, RoutedEventArgs e)
    {
        if (AuthStore.Instance.CanBarcode) Main.NavigateTo(new BarcodePage());
    }

    private void BtnCash_Click(object sender, RoutedEventArgs e)
    {
        if (AuthStore.Instance.CanPayments) Main.NavigateTo(new TransactionPage());
    }

    private void BtnSale_Click(object sender, RoutedEventArgs e)
    {
        if (AuthStore.Instance.CanSales) Main.NavigateTo(new SalePage());
    }

    private void BtnReturn_Click(object sender, RoutedEventArgs e)
    {
        if (AuthStore.Instance.CanReturns) Main.NavigateTo(new ReturnPage());
    }

    private void BtnSettings_Click(object sender, RoutedEventArgs e)
    {
        if (AuthStore.Instance.CanSettings) Main.NavigateTo(new SettingsPage());
    }

    private void BtnSupply_Click(object sender, RoutedEventArgs e)
    {
        if (AuthStore.Instance.CanSupply) Main.NavigateTo(new SupplyPage());
    }

    private void btnReports_Click(object sender, RoutedEventArgs e)
    {
        if (AuthStore.Instance.CanReports) Main.NavigateTo(new ReportsPage());
    }

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        LoginPage.ClearSavedSession();
        AuthStore.Instance.Logout();
        Main.NavigateTo(new LoginPage());
    }

    private void BtnOverdue_Click(object sender, RoutedEventArgs e)
    {
        // Muddati o'tgan qarzlar — pul/mijoz ma'lumoti, faqat Hisobot ruxsati bo'lganlarga.
        if (!AuthStore.Instance.CanReports) return;

        var window = new OverdueAccountsWindow();
        window.ShowDialog();
    }

    private void BtnUserProfile_Click(object sender, RoutedEventArgs e)
    {
        userProfilePopup.IsOpen = !userProfilePopup.IsOpen;

        var rotation = new DoubleAnimation(
            userProfilePopup.IsOpen ? 180 : 0,
            TimeSpan.FromSeconds(0.2));
        dropdownRotation.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, rotation);
    }

    private async void BtnEditProfile_Click(object sender, RoutedEventArgs e)
    {
        userProfilePopup.IsOpen = false;

        // Resize window for better modal visibility
        this.ResizeWindow(1180, 800, always: true);

        // Load user data
        await ProfileViewModel.LoadUserDataAsync();

        // Show modal overlay
        profileEditOverlay.Visibility = Visibility.Visible;
        mainContent.IsEnabled = false;
    }

    private void BtnAccountSettings_Click(object sender, RoutedEventArgs e)
    {
        userProfilePopup.IsOpen = false;
        if (AuthStore.Instance.CanSettings) Main.NavigateTo(new SettingsPage());
    }

    private void BtnNotifications_Click(object sender, RoutedEventArgs e)
    {
        BtnOverdue_Click(sender, e);
    }

    private async Task LoadUserAvatar()
    {
        try
        {
            var userResponse = await client.Users.GetById(AuthStore.Instance.UserId);
            if (userResponse?.Data?.ProfileImageUrl != null && !string.IsNullOrEmpty(userResponse.Data.ProfileImageUrl))
            {
                var fullUrl = client.FileStorage.GetFullUrl(userResponse.Data.ProfileImageUrl);
                var bitmap = new BitmapImage(new Uri(fullUrl));
                imgUserAvatar.ImageSource = bitmap;
                imgProfilePreview.ImageSource = bitmap;
            }
        }
        catch
        {
            // Keep default avatar on error
        }
    }

    private async void Avatar_Click(object sender, MouseButtonEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Rasm fayllari (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
            Title = "Profil rasmini tanlang"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var objectKey = await client.FileStorage.UploadFileAsync(dialog.FileName);
                if (objectKey != null)
                {
                    ProfileViewModel.TmpImagePath = objectKey;
                    imgProfilePreview.ImageSource = new BitmapImage(new Uri(dialog.FileName));
                }
            }
            catch (Exception ex)
            {
                ProfileViewModel.ErrorMessage = $"Rasm yuklashda xatolik: {ex.Message}";
            }
        }
    }

    private async void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(pwdNewPassword.Password))
        {
            if (pwdNewPassword.Password != pwdConfirmPassword.Password)
            {
                ProfileViewModel.WarningMessage = "Parollar mos kelmadi!";
                return;
            }
            ProfileViewModel.NewPassword = pwdNewPassword.Password;
        }

        var success = await ProfileViewModel.SaveAsync();

        if (success)
        {
            ProfileViewModel.SuccessMessage = "Ma'lumotlar muvaffaqiyatli saqlandi!";
            await LoadUserAvatar();

            await Task.Delay(1500);
            BtnCloseModal_Click(sender, e);
        }
    }

    private void BtnCloseModal_Click(object sender, RoutedEventArgs e)
    {
        profileEditOverlay.Visibility = Visibility.Collapsed;
        mainContent.IsEnabled = true;

        // Restore original window size
        this.ResizeWindow(1180, 800, always: true);

        pwdNewPassword.Clear();
        pwdConfirmPassword.Clear();
    }
}
