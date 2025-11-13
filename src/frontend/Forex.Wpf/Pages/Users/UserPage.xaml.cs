namespace Forex.Wpf.Pages.Users;

using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public partial class UserPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;

    private readonly ForexClient client = App.AppHost!.Services.GetRequiredService<ForexClient>();
    private List<UserResponse> rawUsers = [];
    private ObservableCollection<UserResponse> filteredUsers = [];

    public UserPage()
    {
        InitializeComponent();

        cbRole.GotFocus += CbRole_GotFocus;
        cbRole.SelectionChanged += CbRole_SelectionChanged;
        txtSearch.TextChanged += TxtSearch_TextChanged;
        btnSave.Click += BtnSave_Click;
        btnBack.Click += BtnBack_Click;
        txtPhone.GotFocus += TxtPhone_GotFocus;
        btnUpdate.Click += BtnUpdate_Click;
        tbAccount.LostFocus += TbNumeric_LostFocus;
        tbDebt.LostFocus += TbNumeric_LostFocus;
        tbAccount.GotFocus += TextBox_GotFocus_SelectAll;
        tbDebt.GotFocus += TextBox_GotFocus_SelectAll;
        dgUsers.SelectionChanged += DgUsers_SelectionChanged;
        LoadValyutaType();
        LoadUsers();
        UpdateRoleList();

        FocusNavigator.AttachEnterNavigation(
        [
            txtSearch,
            cbRole,
            txtName,
            txtPhone,
            txtAddress,
            txtDescription,
            cbxValutaType,
            tbDebt,
            tbAccount,
            btnSave
        ]);
    }

    private void DgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgUsers.SelectedItem is UserResponse)
        {
            // Row tanlangan → Update rejimi
            btnSave.Visibility = Visibility.Collapsed;
        }
        else
        {
            // Row tanlanmagan → Save rejimi
            btnSave.Visibility = Visibility.Visible;
            btnUpdate.Visibility = Visibility.Collapsed;

            txtName.Text = "";
            txtPhone.Text = null;
            txtAddress.Text = "";
            txtDescription.Text = "";
            tbDebt.Text = "";
            tbAccount.Text = "";
        }
    }


    private void TextBox_GotFocus_SelectAll(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            tb.Dispatcher.BeginInvoke(new Action(() => tb.SelectAll()));
        }
    }

    private void TbNumeric_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb && decimal.TryParse(tb.Text, out var value))
        {
            tb.Text = value.ToString("N2"); // formatlash
        }
    }

    private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        btnUpdate.IsEnabled = false;
        try
        {
            if (dgUsers.SelectedItem is not UserResponse user)
                return;

            // Foydalanuvchi yangilash uchun request tuzamiz
            var updateUser = new UserRequest
            {
                Id = user.Id,
                Name = txtName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Role = Enum.TryParse<UserRole>(cbRole.SelectedItem?.ToString(), out var role) ? role : user.Role,
                Accounts = []
            };

            // Agar account fieldlari mavjud bo‘lsa
            if (decimal.TryParse(tbAccount.Text, out decimal accountBalance) &&
                decimal.TryParse(tbDebt.Text, out decimal discount))
            {
                updateUser.Accounts.Add(new UserAccount
                {
                    CurrencyId = (long)cbxValutaType.SelectedValue,
                    OpeningBalance = GetOpeningBalance(),  // Yangi metod
                    Discount = 0
                });
            }


            // Update chaqirish
            var response = await client.Users.Update(updateUser);

            if (response.IsSuccess)
            {
                LoadUsers(); // qaytadan listni yangilash
            }
            else
            {
                var vm = new SemiProductViewModel
                {
                    ErrorMessage = string.Empty
                };
                vm.ErrorMessage = $"Foydalanuvchining ma'lumotlarini o'zgartirmoqchimisiz?";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Yangilashda xatolik:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            btnUpdate.IsEnabled = true;
            btnSave.Visibility = Visibility.Visible;
            btnUpdate.Visibility = Visibility.Collapsed;
            ClearForm();
        }
    }


    private async void LoadValyutaType()
    {
        try
        {
            var response = await client.Currencies.GetAllAsync();
            if (!response.IsSuccess)
            {
                MessageBox.Show("Valyutani yuklashda xatolik");
                return;
            }

            cbxValutaType.ItemsSource = response.Data?.ToList();

            // Faqat symbol ko‘rinsin
            cbxValutaType.DisplayMemberPath = "Code";

            // SelectedValue sifatida Id ishlatiladi
            cbxValutaType.SelectedValuePath = "Id";

            cbxValutaType.SelectedItem = response.Data!.FirstOrDefault(v => v.IsDefault);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Valyuta turlarini yuklashda xatolik:\n" + ex.Message);
        }
    }
    private async void LoadUsers()
    {
        try
        {
            FilteringRequest request = new()
            {
                Filters = new()
                {
                    ["accounts"] = ["include"],
                    ["accounts"] = ["include:currency"]
                }
            };

            var response = await client.Users.Filter(request).Handle();
            rawUsers = response.Data?.OrderByDescending(u => u.Id).ToList() ?? [];

            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Foydalanuvchilarni yuklashda xatolik:\n" + ex.Message);
        }
    }

    private void UpdateRoleList()
    {
        var roles = Enum.GetNames<UserRole>().ToList();
        cbRole.ItemsSource = roles;
        cbRole.SelectedItem = roles[0];
    }


    private void ApplyFilters()
    {
        string query = txtSearch.Text.Trim().ToLower();
        string selectedRole = cbRole.SelectedItem?.ToString() ?? "";

        var filtered = rawUsers.AsEnumerable();

        // 🔹 UserRole bo‘yicha filter
        if (!string.IsNullOrWhiteSpace(selectedRole) && selectedRole != "User")
        {
            filtered = filtered.Where(u => u.Role.ToString() == selectedRole);
        }
        // Agar User bo‘lsa → hech qanday filter ishlamaydi (hammasi chiqadi)

        // 🔹 Search bo‘yicha filter
        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(u =>
                (u.Name?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                (u.Phone?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                (u.Address?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                (u.Description?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false));
        }

        filteredUsers = new ObservableCollection<UserResponse>(filtered);
        dgUsers.ItemsSource = filteredUsers;
    }
    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void CbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilters();

        // Rol tanlanmagan bo'lsa — hammasi yashirilsin
        if (cbRole.SelectedItem is not string role || string.IsNullOrWhiteSpace(role))
        {
            brValutaType.Visibility = Visibility.Collapsed;
            brDebt.Visibility = Visibility.Collapsed;
            brAccount.Visibility = Visibility.Collapsed;
            btnSave.Visibility = Visibility.Collapsed;
            return;
        }

        bool isUser = role.Equals("User", StringComparison.OrdinalIgnoreCase);

        // Qarzdorlik: faqat "User" bo'lmaganda ko'rinsin
        brDebt.Visibility = isUser ? Visibility.Collapsed : Visibility.Visible;

        // Qolgan elementlar ham "User" bo'lmaganda ko'rinsin
        brValutaType.Visibility = isUser ? Visibility.Collapsed : Visibility.Visible;
        brAccount.Visibility = isUser ? Visibility.Collapsed : Visibility.Visible;
        btnSave.Visibility = isUser ? Visibility.Collapsed : Visibility.Visible;
    }
    private void CbRole_GotFocus(object sender, RoutedEventArgs e)
    {
        LoadUsers();
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var roleText = cbRole.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(roleText) || !Enum.TryParse<UserRole>(roleText, out var role))
            {
                MessageBox.Show("Rol tanlanmagan yoki noto‘g‘ri.");
                return;
            }

            var request = new UserRequest
            {
                Name = txtName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Role = role,
                Accounts =
                [
                   new UserAccount
                   {
        CurrencyId = (long)(cbxValutaType.SelectedValue ?? 0),
        OpeningBalance = GetOpeningBalance(),  // Yangi metod
        Discount = 0  // Endi ishlatilmaydi
                   }
                ]
            };

            var response = await client.Users.Create(request).Handle();
            if (response.Data > 0)
            {
                ClearForm();
                LoadUsers();
            }
            else
            {
                MessageBox.Show("Foydalanuvchini qo‘shishda xatolik.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Xatolik: " + ex.Message);
        }
    }

    private void ClearForm()
    {
        cbRole.SelectedIndex = 0;
        txtName.Text = "";
        txtPhone.Text = "";
        txtAddress.Text = "";
        txtDescription.Text = "";
        tbDebt.Text = "";
        tbAccount.Text = "";
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }

    private void CbRole_Loaded(object sender, RoutedEventArgs e)
    {
        if (cbRole.Template.FindName("PART_EditableTextBox", cbRole) is TextBox tb)
            tb.TextChanged += CbRole_EditableTextBox_TextChanged;
    }

    private void CbRole_EditableTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox tb) return;

        string text = tb.Text ?? string.Empty;
        cbRole.IsDropDownOpen = true;

        var roles = Enum.GetNames<UserRole>().ToList();
        roles.Insert(0, "");

        cbRole.ItemsSource = string.IsNullOrWhiteSpace(text)
            ? roles
            : [.. roles.Where(r => r.Contains(text, StringComparison.InvariantCultureIgnoreCase))];

        tb.SelectionStart = tb.Text!.Length;
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

    private void BtnEdit_Click(object sender, RoutedEventArgs e)
    {
        long userId;
        if (dgUsers.SelectedItem is not UserResponse user) return;
        userId = user.Id;
        LoadingUser(userId);

    }

    private async void LoadingUser(long userId)
    {
        var exitUser = await client.Users.GetById(userId);
        var user = exitUser.Data;
        cbRole.SelectedItem = user!.Role.ToString();
        txtName.Text = user.Name;
        txtPhone.Text = user.Phone;
        txtAddress.Text = user.Address;
        txtDescription.Text = user.Description;

        if (user.Accounts != null && user.Accounts.Count > 0)
        {
            var account = user.Accounts[0];
            cbxValutaType.SelectedValue = account.CurrencyId;

            if (account.OpeningBalance < 0)
            {
                tbDebt.Text = Math.Abs(account.OpeningBalance).ToString("N2");
                tbAccount.Text = "";
            }
            else if (account.OpeningBalance > 0)
            {
                tbAccount.Text = account.OpeningBalance.ToString("N2");
                tbDebt.Text = "";
            }
            else
            {
                tbDebt.Text = "";
                tbAccount.Text = "";
            }
        }
        else
        {
            cbxValutaType.SelectedIndex = 0;
            tbDebt.Text = "";
            tbAccount.Text = "";
        }

        btnSave.Visibility = Visibility.Collapsed;
        btnUpdate.Visibility = Visibility.Visible;
    }

    [RelayCommand]
    private void EditUser(UserResponse response)
    {
        var vm = new SemiProductViewModel { ErrorMessage = string.Empty };
        vm.ErrorMessage = $"{response.Name}ni ma'lumotlarini o'zgartirmoqchimisiz?";

        LoadingUser(response.Id);
    }

    [RelayCommand]
    private static void RemoveUser(UserResponse response)
    {
        var vm = new SemiProductViewModel { ErrorMessage = string.Empty };
        vm.ErrorMessage = $"{response.Name}ni ma'lumotlarini o‘chirmoqchimisiz?";
    }

    [RelayCommand]
    private static void SaveUser(UserResponse response)
    {
        response.IsEditing = false;
    }

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex Digits();

    private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, @"^[0-9.,]+$");
    }

    private void NumericTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string text = (string)e.DataObject.GetData(typeof(string));
            if (!Regex.IsMatch(text, @"^[0-9.,]+$"))
                e.CancelCommand();
        }
        else
        {
            e.CancelCommand();
        }
    }

    private void TbDebt_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(tbDebt.Text) && tbDebt.Text.Trim() != "0" && tbDebt.Text.Trim() != "," && tbDebt.Text.Trim() != ".")
        {
            tbAccount.Text = "";
        }
    }

    private void TbAccount_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(tbAccount.Text) && tbAccount.Text.Trim() != "0" && tbAccount.Text.Trim() != "," && tbAccount.Text.Trim() != ".")
        {
            tbDebt.Text = "";
        }
    }
    private decimal GetOpeningBalance()
    {
        string debtText = tbDebt.Text?.Trim();
        string accountText = tbAccount.Text?.Trim();

        // 1. Qarzdorlikni tekshirish
        if (!string.IsNullOrWhiteSpace(debtText) &&
            decimal.TryParse(debtText,
                out decimal debt) &&
            debt > 0)
        {
            return -debt; // Qarzdorlik → minus
        }

        // 2. Haqdorlikni tekshirish
        if (!string.IsNullOrWhiteSpace(accountText) &&
            decimal.TryParse(accountText,
                out decimal balance) &&
            balance > 0)
        {
            return balance; // Haqdorlik → plus
        }

        // 3. Hech narsa kiritilmagan yoki 0
        return 0;
    }
}

