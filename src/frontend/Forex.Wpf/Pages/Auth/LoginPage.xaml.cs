namespace Forex.Wpf.Pages.Auth;

using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Text.Json;

/// <summary>
/// Interaction logic for LoginPage.xaml
/// </summary>
public partial class LoginPage : Page
{
    private readonly LoginViewModel viewModel;
    private const string SessionFileName = "session.json";

    public LoginPage()
    {
        InitializeComponent();
        var services = App.AppHost!.Services;
        viewModel = services.GetRequiredService<LoginViewModel>();
        DataContext = viewModel;

        tbLogin.Focus();

        FocusNavigator.RegisterElements([
            tbLogin,
            pbPassword,
            chRemember,
            btnLogin,
            ]);

        Loaded += LoginPage_Loaded;
    }

    private async void LoginPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            string path = GetSessionFilePath();

            if (File.Exists(path))
            {
                string json = await File.ReadAllTextAsync(path);
                var session = JsonSerializer.Deserialize<SavedSession>(json);

                if (session != null && (DateTime.Now - session.LoginTime).TotalHours < 5)
                {
                    tbLogin.Text = session.Username;
                    pbPassword.Password = session.Password; // Note: In real app, password should be encrypted
                    chRemember.IsChecked = true;

                    // Auto login
                    await PerformLogin(session.Username, session.Password);
                }
            }
        }
        catch { /* Ignore errors during auto-login check */ }
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        lblError.Visibility = Visibility.Collapsed;
        string login = tbLogin.Text.Trim();
        string password = pbPassword.Password;

        await PerformLogin(login, password);
    }

    private async Task PerformLogin(string login, string password)
    {
        var success = await viewModel.LoginAsync(login, password);

        if (success)
        {
            if (chRemember.IsChecked == true)
            {
                try
                {
                    var session = new SavedSession
                    {
                        Username = login,
                        Password = password,
                        LoginTime = DateTime.Now
                    };

                    string path = GetSessionFilePath();
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    string json = JsonSerializer.Serialize(session);
                    await File.WriteAllTextAsync(path, json);
                }
                catch { /* Ignore save error */ }
            }
            else
            {
                // Clear saved session if user unchecks
                ClearSavedSession();
            }

            NavigationService?.Navigate(new HomePage());
        }
        else
        {
            if (viewModel.LastFailureWasConnectionError)
            {
                var urlWindow = App.AppHost!.Services.GetRequiredService<ServerUrlWindow>();
                urlWindow.Owner = Application.Current.MainWindow;

                if (urlWindow.ShowDialog() == true)
                {
                    await PerformLogin(login, password);
                    return;
                }
            }

            lblError.Text = viewModel.ErrorMessage;
            lblError.Visibility = Visibility.Visible;
        }
    }

    public class SavedSession
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public DateTime LoginTime { get; set; }
    }

    public static void ClearSavedSession()
    {
        string path = GetSessionFilePath();
        if (!File.Exists(path)) return;

        try
        {
            File.Delete(path);
        }
        catch (IOException ex)
        {
            MessageBox.Show($"Saqlangan login ma'lumotlarini o'chirishda xatolik: {ex.Message}", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show($"Saqlangan login ma'lumotlarini o'chirishga ruxsat yo'q: {ex.Message}", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private static string GetSessionFilePath()
    {
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ForexApp");
        return Path.Combine(folder, SessionFileName);
    }
}
