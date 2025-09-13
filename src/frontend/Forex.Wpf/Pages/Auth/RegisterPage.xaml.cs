namespace Forex.Wpf.Pages.Auth;

using Forex.ClientService;
using Forex.Wpf.Windows;
using System;
using System.Windows;
using System.Windows.Controls;

public partial class RegisterPage : Page
{
    private readonly ForexClient client;

    public RegisterPage(ForexClient client)
    {
        InitializeComponent();
        this.client = client;
        tbName.Focus();
    }

    private async void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        HideMessages();

        var name = tbName.Text.Trim();
        var email = tbEmail.Text.Trim();
        var phone = tbPhone.Text.Trim();
        var pass = pbPassword.Password;
        var confirm = pbConfirm.Password;

        if (string.IsNullOrWhiteSpace(name) ||
            (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone)) ||
            string.IsNullOrWhiteSpace(pass))
        {
            ShowError("Name, contact and password are required.");
            return;
        }

        if (!string.Equals(pass, confirm, StringComparison.Ordinal))
        {
            ShowError("Passwords do not match.");
            return;
        }

        try
        {
            var resp = await client.Auth.Register(new()
            {
                Name = name,
                Email = email,
                Phone = phone,
                Password = pass
            });

            if (resp.StatusCode != 200)
            {
                ShowError(resp.Message ?? "Registration failed.");
                return;
            }

            lblOk.Text = "Account created successfully. You can log in now.";
            lblOk.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            ShowError("Error: " + ex.Message);
        }
    }

    private void BtnGoLogin_Click(object sender, RoutedEventArgs e)
    {
        ((MainWindow)Application.Current.MainWindow).NavigateTo(new LoginPage(client));
    }

    private void HideMessages()
    {
        lblOk.Visibility = Visibility.Collapsed;
        lblError.Visibility = Visibility.Collapsed;
    }

    private void ShowError(string msg)
    {
        lblError.Text = msg;
        lblError.Visibility = Visibility.Visible;
    }
}
