namespace Forex.Wpf.Windows;

using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Common.Services;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

public partial class UserWindow : Window
{
    private long somId;
    private readonly ForexClient client;
    private readonly IMapper mapper;

    public UserViewModel? user;
    public UserWindow()
    {
        InitializeComponent();

        client = App.AppHost!.Services.GetRequiredService<ForexClient>();
        mapper = App.AppHost!.Services.GetRequiredService<IMapper>();

        txtPhone.Focus();

        // Enter bosilganda navbatdagi elementga o'tish
        FocusNavigator.RegisterElements([
            txtName,
            txtPhone,
            txtAddress,
            txtBeginningSum,
            txtBeginningSum2,
            txtDescription,
            btnSave
        ]);

        // Valyuta turlarini yuklash
        _ = LoadValyutaTypeAsync();
    }

    private async Task LoadValyutaTypeAsync()
    {
        try
        {
            var valyutaTypes = await client.Currencies.GetAllAsync().Handle();

            somId = valyutaTypes.Data?.FirstOrDefault(v =>
                v.Symbol.Equals("UZS", StringComparison.OrdinalIgnoreCase))?.Id ?? 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Valyuta turlarini yuklashda xatolik:\n{ex.Message}",
                "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (somId == 0)
                await LoadValyutaTypeAsync();

            decimal balance = 0;

            if (!string.IsNullOrWhiteSpace(txtBeginningSum.Text) &&
                decimal.TryParse(txtBeginningSum.Text, out var qarz))
                balance = -qarz;
            else if (!string.IsNullOrWhiteSpace(txtBeginningSum2.Text) &&
                     decimal.TryParse(txtBeginningSum2.Text, out var haq))
                balance = haq;

            var userRequest = new UserRequest
            {
                Name = txtName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                Description = txtDescription.Text.Trim(),
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

            var response = await client.Users.Create(userRequest).Handle();

            if (response.IsSuccess)
            {
                userRequest.Id = response.Data ?? 0;
                user = mapper.Map<UserViewModel>(userRequest);
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Mijozni yaratib bo‘lmadi.", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik yuz berdi: {ex.Message}",
                "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
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

            tb.Dispatcher.BeginInvoke(new Action(() =>
            {
                tb.SelectionStart = tb.Text.Length;
                tb.SelectionLength = 0;
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
                formatted += digits.Substring(3, Math.Min(2, digits.Length - 3));
            if (digits.Length > 5)
                formatted += string.Concat(" ", digits.AsSpan(5, Math.Min(3, digits.Length - 5)));
            if (digits.Length > 8)
                formatted += string.Concat(" ", digits.AsSpan(8, Math.Min(2, digits.Length - 8)));
            if (digits.Length > 10)
                formatted += string.Concat(" ", digits.AsSpan(10, Math.Min(2, digits.Length - 10)));

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
