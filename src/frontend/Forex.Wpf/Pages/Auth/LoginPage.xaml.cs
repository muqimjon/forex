namespace Forex.Wpf.Pages.Auth;

using Forex.ClientService;
using Forex.Wpf.Pages.Home;
using System;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for LoginPage.xaml
/// </summary>
public partial class LoginPage : Page
{
    private readonly ForexClient client;

    public LoginPage(ForexClient client)
    {
        InitializeComponent();
        this.client = client;
        tbLogin.Focus();
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        lblError.Visibility = Visibility.Collapsed;

        var login = tbLogin.Text.Trim();
        var password = pbPassword.Password;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Login va parol majburiy.");
            return;
        }

        try
        {
            var resp = await client.Auth.Login(new() { EmailOrPhone = login, Password = password });

            if (resp.StatusCode != 200)
            {
                ShowError(resp.Message ?? "Login muvaffaqiyatsiz.");
                return;
            }

            NavigationService?.Navigate(new HomePage(client));
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void BtnGoRegister_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new RegisterPage(client));
    }

    private void ShowError(string msg)
    {
        lblError.Text = msg;
        lblError.Visibility = Visibility.Visible;
    }
}

