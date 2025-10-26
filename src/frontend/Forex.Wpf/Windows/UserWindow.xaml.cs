namespace Forex.Wpf.Windows;

using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Interaction logic for UserWindow.xaml
/// </summary>
public partial class UserWindow : Window
{
    public UserWindow()
    {
        InitializeComponent();
        txtName.Focus();

        FocusNavigator.AttachEnterNavigation(
[
            txtName,
            txtPhone,
            txtAddress,
            txtBeginningSum,
            txtBeginningSum2,
            txtDescription,
            btnSave
]);

    }
    long somId;
    private async void LoadValyutaType()
    {
        try
        {

            var client = App.AppHost!.Services.GetRequiredService<ForexClient>();
            var valyutaTypes = await client.Currencies.GetAllAsync().Handle();
            somId = valyutaTypes.Data!.FirstOrDefault(v =>
                v.Symbol.Equals("UZS", StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Valyuta turlarini yuklashda xatolik:\n" + ex.Message);
        }
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            LoadValyutaType();
            // 💰 Qarzdorlik yoki haqdorlikni aniqlaymiz
            decimal balance = 0;

            if (!string.IsNullOrWhiteSpace(txtBeginningSum.Text) &&
                decimal.TryParse(txtBeginningSum.Text, out var qarz))
            {
                balance = -qarz; // Qarzdorlik — manfiy
            }
            else if (!string.IsNullOrWhiteSpace(txtBeginningSum2.Text) &&
                     decimal.TryParse(txtBeginningSum2.Text, out var haq))
            {
                balance = haq; // Haqdorlik — musbat
            }



            var userRequest = new UserRequest
            {
                Name = txtName.Text,
                Phone = txtPhone.Text,
                Address = txtAddress.Text,
                Description = txtDescription.Text,
                Role = UserRole.Mijoz,
                Accounts =
        [
            new UserAccount
            {
                CurrencyId = somId,
                OpeningBalance = balance,
                Discount = 0
            }
        ]
            };

            // 🧩 Global client orqali ishlaymiz
            var response = await App.AppHost!.Services.GetRequiredService<ForexClient>().Users.Create(userRequest).Handle();

            if (response.IsSuccess)
            {
                DialogResult = true;
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik yuz berdi: {ex.Message}", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }

    private void TxtPhone_TextChanged(object sender, TextChangedEventArgs e)
    {
        FormatPhoneNumber((sender as TextBox)!);
    }

    private void TxtPhone_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            if (string.IsNullOrWhiteSpace(tb.Text) || !tb.Text.Replace("+", "").StartsWith("998"))
            {
                tb.Text = "+998 ";
            }

            // Fokus paytida matn tanlanib qolmasligi uchun:
            tb.Dispatcher.BeginInvoke(new Action(() =>
            {
                tb.SelectionStart = tb.Text.Length;   // kursorni oxiriga qo‘yish
                tb.SelectionLength = 0;               // tanlovni olib tashlash
            }), System.Windows.Threading.DispatcherPriority.Input);
        }
    }

    private void FormatPhoneNumber(TextBox textBox)
    {
        if (textBox == null) return;
        string text = textBox.Text?.Trim() ?? string.Empty;
        string digits = Digits().Replace(text, "");
        textBox.TextChanged -= TxtPhone_TextChanged;
        try
        {

            string formatted = "+998 ";
            if (digits.Length > 3)
            {
                formatted += digits.Substring(3, Math.Min(2, digits.Length - 3));
            }
            if (digits.Length > 5)
            {
                formatted += string.Concat(" ", digits.AsSpan(5, Math.Min(3, digits.Length - 5)));
            }
            if (digits.Length > 8)
            {
                formatted += string.Concat(" ", digits.AsSpan(8, Math.Min(2, digits.Length - 8)));
            }
            if (digits.Length > 10)
            {
                formatted += string.Concat(" ", digits.AsSpan(10, Math.Min(2, digits.Length - 10)));
            }
            textBox.Text = formatted.TrimEnd();
            textBox.SelectionStart = textBox.Text.Length;
        }
        finally
        {
            textBox.TextChanged += TxtPhone_TextChanged;
        }
    }

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex Digits();
}
