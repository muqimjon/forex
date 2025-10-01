namespace Forex.Wpf.Pages.Users;

using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Models.Users;
using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.SemiProducts.ViewModels;
using Forex.Wpf.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

public partial class UserPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;

    private readonly ForexClient client = App.Client;
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
        txtPhone.GotFocus += txtPhone_GotFocus;
        btnUpdate.Click += BtnUpdate_Click;
        tbAccount.LostFocus += TbNumeric_LostFocus;
        tbDiscount.LostFocus += TbNumeric_LostFocus;
        tbAccount.GotFocus += TextBox_GotFocus_SelectAll;
        tbDiscount.GotFocus += TextBox_GotFocus_SelectAll;
        dgUsers.SelectionChanged += dgUsers_SelectionChanged;
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
            tbDiscount,
            tbAccount,
            btnSave
        ]);
    }

    private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
            tbDiscount.Text = "";
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
                Role = Enum.TryParse<Role>(cbRole.SelectedItem?.ToString(), out var role) ? role : user.Role,
                CurrencyBalances = new List<CurrencyBalanceRequest>()
            };
            decimal accountBalance = 0m;
            decimal discount = 0m;

            // Agar account fieldlari mavjud bo‘lsa
            if (decimal.TryParse(tbAccount.Text, out accountBalance) &&
                decimal.TryParse(tbDiscount.Text, out discount))
            {
                updateUser.CurrencyBalances.Add(new CurrencyBalanceRequest
                {
                    CurrencyId = (long)cbxValutaType.SelectedValue,
                    Balance = accountBalance,
                    Discount = discount
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
                var vm = new SemiProductViewModel();
                vm.ErrorMessage = string.Empty;
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
            var valyutaTypes = await client.Currency.GetAll();
            cbxValutaType.ItemsSource = valyutaTypes.Data?.ToList();

            // Faqat symbol ko‘rinsin
            cbxValutaType.DisplayMemberPath = "Symbol";

            // SelectedValue sifatida Id ishlatiladi
            cbxValutaType.SelectedValuePath = "Id";
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
            var response = await client.Users.GetAll();
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
        var roles = Enum.GetNames<Role>().ToList();
        cbRole.ItemsSource = roles;
        cbRole.SelectedItem = roles[0];
    }


    private void ApplyFilters()
    {
        string query = txtSearch.Text.Trim().ToLower();
        string selectedRole = cbRole.SelectedItem?.ToString() ?? "";

        var filtered = rawUsers.AsEnumerable();

        // 🔹 Role bo‘yicha filter
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

        if (cbRole.SelectedItem is not string role || string.IsNullOrWhiteSpace(role))
        {
            brValutaType.Visibility = Visibility.Collapsed;
            brDiscount.Visibility = Visibility.Collapsed;
            brAccount.Visibility = Visibility.Collapsed;
            btnSave.Visibility = Visibility.Collapsed;
            return;
        }

        brDiscount.Visibility = role == "Mijoz" ? Visibility.Visible : Visibility.Collapsed;
        brAccount.Visibility = role == "User" ? Visibility.Collapsed : Visibility.Visible;
        btnSave.Visibility = role == "User" ? Visibility.Collapsed : Visibility.Visible;

        brValutaType.Visibility =
        (role == "Mijoz" || role == "Taminotchi" || role == "Hodim")
        ? Visibility.Visible
        : Visibility.Collapsed;


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
            if (string.IsNullOrWhiteSpace(roleText) || !Enum.TryParse<Role>(roleText, out var role))
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
                CurrencyBalances = new List<CurrencyBalanceRequest>
                {
                    new CurrencyBalanceRequest
                    {
                        CurrencyId = (long)(cbxValutaType.SelectedValue ?? 0),
                        Balance = decimal.TryParse(tbAccount.Text, out var bal) ? bal : 0,
                        Discount = decimal.TryParse(tbDiscount.Text, out var disc) ? disc : 0
                    }
                }
            };

            var response = await client.Users.Create(request);
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
        tbDiscount.Text = "";
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

        var roles = Enum.GetNames<Role>().ToList();
        roles.Insert(0, "");

        cbRole.ItemsSource = string.IsNullOrWhiteSpace(text)
            ? roles
            : [.. roles.Where(r => r.Contains(text, StringComparison.InvariantCultureIgnoreCase))];

        tb.SelectionStart = tb.Text!.Length;
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

    private void btnEdit_Click(object sender, RoutedEventArgs e)
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
        cbRole.SelectedItem = user.Role.ToString();
        txtName.Text = user.Name;
        txtPhone.Text = user.Phone;
        txtAddress.Text = user.Address;
        txtDescription.Text = user.Description;

        if (user.Accounts != null && user.Accounts.Count > 0)
        {
            var account = user.Accounts[0];
            cbxValutaType.SelectedValue = account.CurrencyId;
            tbAccount.Text = account.Balance.ToString("N2");
            tbDiscount.Text = account.Discount.ToString("N2");
        }
        else
        {
            cbxValutaType.SelectedIndex = 0;
            tbAccount.Text = "0";
            tbDiscount.Text = "0";
        }

        btnSave.Visibility = Visibility.Collapsed;
        btnUpdate.Visibility = Visibility.Visible;
    }

    [RelayCommand]
    private void EditUser(UserResponse response)
    {
        var vm = new SemiProductViewModel();
        vm.ErrorMessage = string.Empty;
        vm.ErrorMessage = $"{response.Name}ni ma'lumotlarini o'zgartirmoqchimisiz?";

        LoadingUser(response.Id);
    }

    [RelayCommand]
    private void RemoveUser(UserResponse response)
    {
        var vm = new SemiProductViewModel();
        vm.ErrorMessage = string.Empty;
        vm.ErrorMessage = $"{response.Name}ni ma'lumotlarini o‘chirmoqchimisiz?";
        DeleteUser(response.Id);
    }

    [RelayCommand]
    private void SaveUser(UserResponse response)
    {
        response.IsEditing = false;
        var vm = new SemiProductViewModel();
        vm.SuccessMessage = string.Empty;
        vm.SuccessMessage = $"B{response.Name}ni ma'lumotlari saqlandi";
    }
    private async void DeleteUser(long userId)
    {
        try
        {
            var vm = new SemiProductViewModel();
            vm.SuccessMessage = string.Empty;
            var response = await client.Users.Delete(userId);
            if (response.IsSuccess)
            {
                LoadUsers();
                vm.SuccessMessage = $"Foydalanuvchini o'chirish muvaffaqiyatli bajarildi.";
            }
            else
            {
                vm.ErrorMessage = $"Foydalanuvchini o'chirishda xatolik.";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Xatolik: " + ex.Message);
        }
    }
}

