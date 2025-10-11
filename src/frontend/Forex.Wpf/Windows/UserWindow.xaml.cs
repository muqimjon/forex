using Forex.ClientService.Enums;
using Forex.ClientService.Interfaces;
using Forex.ClientService.Models.Users;
using Forex.Wpf.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Forex.Wpf.Windows;

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
            var valyutaTypes = await App.Client.Currency.GetAll();
            somId = valyutaTypes.Data.FirstOrDefault(v =>
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
                Role = Role.Mijoz,
                CurrencyBalances = new List<CurrencyBalanceRequest>
        {
            new CurrencyBalanceRequest
            {
                CurrencyId = somId,
                Balance = balance,
                Discount = 0
            }
        }
            };

            // 🧩 Global client orqali ishlaymiz
            var createdUser = await App.Client.Users.Create(userRequest);

            if (createdUser != null)
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

    private void txtPhone_TextChanged(object sender, TextChangedEventArgs e)
    {
        FormatPhoneNumber(sender as TextBox);
    }
    private void txtPhone_GotFocus(object sender, RoutedEventArgs e)
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
        string digits = Regex.Replace(text, @"[^\d]", "");
        textBox.TextChanged -= txtPhone_TextChanged;
        try
        {

            string formatted = "+998 ";
            if (digits.Length > 3)
            {
                formatted += digits.Substring(3, Math.Min(2, digits.Length - 3));
            }
            if (digits.Length > 5)
            {
                formatted += " " + digits.Substring(5, Math.Min(3, digits.Length - 5));
            }
            if (digits.Length > 8)
            {
                formatted += " " + digits.Substring(8, Math.Min(2, digits.Length - 8));
            }
            if (digits.Length > 10)
            {
                formatted += " " + digits.Substring(10, Math.Min(2, digits.Length - 10));
            }
            textBox.Text = formatted.TrimEnd();
            textBox.SelectionStart = textBox.Text.Length;
        }
        finally
        {
            textBox.TextChanged += txtPhone_TextChanged;
        }
    }


}
