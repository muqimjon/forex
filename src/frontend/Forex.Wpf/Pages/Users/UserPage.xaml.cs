namespace Forex.Wpf.Pages.Users;

using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Models.Users;
using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            tbDiscount,
            tbAccount,
            btnSave
        ]);
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

        var filtered = rawUsers.Where(u =>
            (string.IsNullOrWhiteSpace(selectedRole) || u.Role.ToString() == selectedRole) &&
            (
                string.IsNullOrWhiteSpace(query) ||
                (u.Name?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                (u.Phone?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                (u.Address?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                (u.Description?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false)
            )
        ).ToList();

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
            brDiscount.Visibility = Visibility.Collapsed;
            brAccount.Visibility = Visibility.Collapsed;
            btnSave.Visibility = Visibility.Collapsed;
            return;
        }

        brDiscount.Visibility = role == "Mijoz" ? Visibility.Visible : Visibility.Collapsed;
        brAccount.Visibility = role == "User" ? Visibility.Collapsed : Visibility.Visible;
        btnSave.Visibility = role == "User" ? Visibility.Collapsed : Visibility.Visible;
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
                Balance = decimal.TryParse(tbAccount.Text, out var balance) ? balance : 0
            };

            var response = await client.Users.Create(request);
            if (response.Data > 0)
            {
                MessageBox.Show("Foydalanuvchi muvaffaqiyatli qo‘shildi.");
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

}
