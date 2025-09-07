namespace Forex.Wpf.Pages.Users;

using Forex.Wpf.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Interaction logic for UserPage.xaml
/// </summary>
public partial class UserPage : Page
{
    private readonly List<string> _allRoles = new() { "Mijoz", "Yetkazuvchi", "Hodim" };

    public UserPage()
    {
        InitializeComponent();

        // boshlang'ich ro'yxatni qo'yamiz
        cbRole.ItemsSource = _allRoles;
        cbRole.SelectedIndex = -1;
        txtSearch.Focus();
    }

    // ComboBox loadingda ichki TextBoxni topib TextChanged ulaymiz
    private void cbRole_Loaded(object sender, RoutedEventArgs e)
    {
        if (cbRole.Template.FindName("PART_EditableTextBox", cbRole) is TextBox tb)
        {
            tb.TextChanged += CbRole_EditableTextBox_TextChanged;
        }
    }

    // Text o'zgarganda ishlaydi — filterlaymiz va dropdownni ochamiz
    private void CbRole_EditableTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox tb) return;

        string text = tb.Text ?? string.Empty;
        // dropdownni har doim ochamiz (agar kerak bo'lsa)
        cbRole.IsDropDownOpen = true;

        // bo'sh matn bo'lsa — to'liq ro'yxatni qaytar
        if (string.IsNullOrWhiteSpace(text))
        {
            cbRole.ItemsSource = _allRoles;
            return;
        }

        // filtr — case insensitive
        var filtered = _allRoles
            .Where(r => r.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0)
            .ToList();

        // agar hech narsa topilmasa — foydalanuvchiga yozilganni saqlab qolamiz (yoki "No results")
        if (filtered.Count == 0)
        {
            // agar xohlasangiz, filtered.Add("Hech narsa topilmadi");
        }

        // ItemsSource ni yangilaymiz
        cbRole.ItemsSource = filtered;

        // caret'ni oxiriga qo'yish
        tb.SelectionStart = tb.Text.Length;
    }
    private void btnBack_Click(object sender, RoutedEventArgs e)
    {
        if (this.NavigationService != null && this.NavigationService.CanGoBack)
        {
            this.NavigationService.GoBack(); // Orqaga qaytadi
        }
        else
        {
            // Agar back yo'q bo'lsa to'g'ridan-to'g'ri MainWindow ga qaytish
            MainWindow main = new MainWindow();
            Application.Current.MainWindow = main;
            main.Show();

            // Hozirgi oynani yopamiz
            Window.GetWindow(this)?.Close();
        }
    }

    private void cbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cbRole.SelectedItem is null) return;

        string role = cbRole.SelectedItem.ToString() ?? string.Empty;

        switch (role)
        {
            case "Mijoz":
                brDiscount.Visibility = Visibility.Visible;
                brAccount.Visibility = Visibility.Visible;
                btnSave.Visibility = Visibility.Visible;
                spAccount.Visibility = Visibility.Visible;
                break;

            case "Yetkazuvchi":
                brDiscount.Visibility = Visibility.Collapsed;
                brAccount.Visibility = Visibility.Visible;
                btnSave.Visibility = Visibility.Visible;
                spAccount.Visibility = Visibility.Visible;
                break;

            case "Hodim":
                brDiscount.Visibility = Visibility.Collapsed;
                brAccount.Visibility = Visibility.Visible;
                btnSave.Visibility = Visibility.Visible;
                spAccount.Visibility = Visibility.Visible;
                break;

            default:
                brDiscount.Visibility = Visibility.Collapsed;
                brAccount.Visibility = Visibility.Collapsed;
                break;
        }
    }

    private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            switch (sender)
            {
                case var _ when sender == txtSearch: cbRole.Focus(); break;
                case var _ when sender == cbRole: txtName.Focus(); break;
                case var _ when sender == txtName: txtPhone.Focus(); break;
                case var _ when sender == txtPhone: txtAddress.Focus(); break;
                case var _ when sender == txtAddress: txtDescription.Focus(); break;
                case var _ when sender == txtDescription:
                    if (brDiscount.Visibility == Visibility.Visible)
                        tbDiscount.Focus();
                    else if (brAccount.Visibility == Visibility.Visible)
                        tbAccount.Focus();
                    else
                        btnSave.Focus();
                    break;

                case var _ when sender == tbDiscount:
                    if (brAccount.Visibility == Visibility.Visible)
                        tbAccount.Focus();
                    else
                        btnSave.Focus();
                    break;
                case var _ when sender == tbAccount: btnSave.Focus(); break;
            }
    }
}
