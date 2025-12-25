namespace Forex.Wpf.Windows;

using Forex.ClientService;
using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Auth;
using System.Windows;
using System.Windows.Controls;

public partial class MainWindow : Window
{
    public ForexClient Client { get; private set; } = default!;

    public MainWindow()
    {
        InitializeComponent();

        // Spinner (yuklanish indikatori) xizmatini ishga tushirish
        SpinnerService.Init(this);

        // 🔥 O'ZGARTIRILGAN JOY:
        // Dastur yuklanishi bilan HomePage ga emas, LoginPage ga yuboramiz
        Loaded += (_, _) => NavigateTo(new LoginPage());
    }

    // Sahifalararo harakatlanish uchun yordamchi metod
    public void NavigateTo(Page page)
    {
        // Debug uchun qaysi sahifaga o'tilayotganini ko'rsatadi
        System.Diagnostics.Debug.WriteLine($"Navigating to: {page.GetType().Name}");

        MainFrame.Navigate(page);
    }

    public void GoBack()
    {
        if (MainFrame.CanGoBack)
            MainFrame.GoBack();
    }
}