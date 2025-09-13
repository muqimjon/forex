namespace Forex.Wpf.Pages.Users;

using Forex.Wpf.Pages.Home;
using Forex.Wpf.Services;
using Forex.Wpf.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

public partial class UserPage : Page
{
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private readonly List<string> _allRoles = ["Mijoz", "Yetkazuvchi", "Hodim"];

    public UserPage()
    {
        InitializeComponent();
        cbRole.ItemsSource = _allRoles;
        cbRole.SelectedIndex = -1;
        txtSearch.Focus();

        FocusNavigatorService.AttachEnterNavigation(
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

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo<HomePage>();
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

        cbRole.ItemsSource = string.IsNullOrWhiteSpace(text)
            ? _allRoles
            : [.. _allRoles.Where(r => r.Contains(text, StringComparison.InvariantCultureIgnoreCase))];

        tb.SelectionStart = tb.Text!.Length;
    }

    private void CbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cbRole.SelectedItem is not string role) return;

        brDiscount.Visibility = role == "Mijoz" ? Visibility.Visible : Visibility.Collapsed;
        brAccount.Visibility = Visibility.Visible;
        btnSave.Visibility = Visibility.Visible;
    }
}
