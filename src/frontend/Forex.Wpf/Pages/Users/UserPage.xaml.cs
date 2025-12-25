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
using System.Collections.ObjectModel;
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
    private bool isCreatingNewUser = false;
    private UserResponse currentUser;

    public UserPage()
    {
        InitializeComponent();

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

        LoadValyutaType();
        LoadUsers();
        UpdateRoleList();

        Loaded += Page_Loaded;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        RegisterFocusNavigation();
        RegisterGlobalShortcuts();
    }

    private void RegisterGlobalShortcuts()
    {
        ShortcutAttacher.RegisterShortcut(
            targetButton: btnBack,
            key: Key.Escape
        );
    }

    private void RegisterFocusNavigation()
    {
        FocusNavigator.RegisterElements([
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

        FocusNavigator.SetFocusRedirect(btnBack, cbRole);
    }

    // 🔴 Input maydonlariga focus berilganda
    private void InputField_GotFocus(object sender, RoutedEventArgs e)
    {
        if (!isCreatingNewUser)
        {
            isCreatingNewUser = true;
            dgUsers.SelectedItem = null; // Selection ni tozalash
            btnSave.Visibility = GetSaveButtonVisibility();
            btnUpdate.Visibility = Visibility.Collapsed;
        }
    }


    // 🔴 Save tugmasi ko'rinishini aniqlash
    private Visibility GetSaveButtonVisibility()
    {
        string role = cbRole.SelectedItem?.ToString() ?? "";
        bool isUser = role.Equals("User", StringComparison.OrdinalIgnoreCase);
        return isUser ? Visibility.Collapsed : Visibility.Visible;
    }

    private void TextBox_GotFocus_SelectAll(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
            tb.Dispatcher.BeginInvoke(new Action(() => tb.SelectAll()));
    }

    private void TbNumeric_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb && decimal.TryParse(tb.Text, out var value))
            tb.Text = value.ToString("N2");
    }

    private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        btnUpdate.IsEnabled = false;
        if (!ValidateUserFields() || !ValidateAdminRestrictions())
        {
            btnUpdate.IsEnabled = true;
            return;
        }

        try
        {
            if (currentUser == null) return;

            var updateUser = new UserRequest
            {
                Id = currentUser.Id,
                Name = txtName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                Role = Enum.TryParse<UserRole>(cbRole.SelectedItem?.ToString(), out var role) ? role : currentUser.Role,

                // 🔴 Admin hodimni tahrirlayotgan bo'lsa, yangi login va parolni olamiz
                UserName = (tglHasAccess.IsChecked == true) ? txtUsername.Text.Trim() : currentUser.UserName,
                // Parol bo'sh bo'lsa, bazadagi eski parol qolaveradi (null yuborilsa o'zgarmaydi)
                Password = (tglHasAccess.IsChecked == true && !string.IsNullOrWhiteSpace(txtPassword.Password))
                            ? txtPassword.Password.Trim() : null,

                Accounts = []
            };

            // Accountni saqlash... (sizning kodingiz)
            if (cbxValutaType.SelectedValue != null)
            {
                updateUser.Accounts.Add(new UserAccount
                {
                    CurrencyId = (long)cbxValutaType.SelectedValue,
                    OpeningBalance = GetOpeningBalance(),
                    Discount = 0
                });
            }

            var response = await client.Users.Update(updateUser);

            if (response.IsSuccess)
            {
                MessageBox.Show("Muvaffaqiyatli yangilandi.", "Ok", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
                ClearForm();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Xatolik: " + ex.Message);
        }
        finally
        {
            btnUpdate.IsEnabled = true;
            btnUpdate.Visibility = Visibility.Collapsed;
            btnSave.Visibility = GetSaveButtonVisibility();
            currentUser = null;
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
            cbxValutaType.DisplayMemberPath = "Code";
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
                    ["accounts"] = ["include:currency"]
                },
                Descending = true
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

        // 🔴 Default qiymat sifatida "User" ni o'rnatish
        cbRole.SelectedItem = "User";
    }

    private void ApplyFilters()
    {
        string query = txtSearch.Text.Trim().ToLower();
        string selectedRole = cbRole.SelectedItem?.ToString() ?? "";

        var filtered = rawUsers.AsEnumerable();

        // 🔴 Filter logikasi: "User" tanlanganida HAMMA ko'rinadi
        if (!string.IsNullOrWhiteSpace(selectedRole) && selectedRole != "User")
        {
            // Boshqa rol tanlanganida faqat o'sha rolni ko'rsatish
            filtered = filtered.Where(u => u.Role.ToString() == selectedRole);
        }
        // "User" tanlanganida yoki bo'sh bo'lganida hamma ko'rinadi

        // Search bo'yicha filter
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

    // AuthStore orqali tizimga kirgan odamni tekshiramiz
    private bool IsLoggedInAsAdmin =>
        ClientService.Services.AuthStore.Instance.Username?.ToLower() == "admin";
    private void CbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilters();

        if (cbRole.SelectedItem is not string role) return;

        bool isUser = role.Equals("User", StringComparison.OrdinalIgnoreCase);
        bool isEmployee = role.Equals("Hodim", StringComparison.OrdinalIgnoreCase);

        // Vizual bloklar boshqaruvi
        brDebt.Visibility = isUser ? Visibility.Collapsed : Visibility.Visible;
        brValutaType.Visibility = isUser ? Visibility.Collapsed : Visibility.Visible;
        brAccount.Visibility = isUser ? Visibility.Collapsed : Visibility.Visible;
        btnSave.Visibility = isUser ? Visibility.Collapsed : Visibility.Visible;

        // 🔴 SHART: Admin kirgan bo'lsa VA "Hodim" tanlansa Toggle chiqadi
        if (IsLoggedInAsAdmin && isEmployee)
        {
            pnlEmployeeAccess.Visibility = Visibility.Visible;
        }
        else
        {
            pnlEmployeeAccess.Visibility = Visibility.Collapsed;
            tglHasAccess.IsChecked = false;
            txtUsername.Text = "";
            txtPassword.Password = "";
        }
    }

    private bool ValidateAdminRestrictions()
    {
        if (tglHasAccess.IsChecked == true)
        {
            string inputUsername = txtUsername.Text.Trim().ToLower();

            // Agar yangi odam bo'lsa yoki tahrirlanayotgan odam eski admin bo'lmasa
            if (inputUsername == "admin")
            {
                if (currentUser == null || currentUser.UserName?.ToLower() != "admin")
                {
                    MessageBox.Show("Yangi hodim uchun 'admin' loginini tanlash mumkin emas!",
                        "Taqiq", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
        }
        return true;
    }
    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateUserFields() || !ValidateAdminRestrictions()) return;

        var roleText = cbRole.SelectedItem?.ToString();

        if (string.IsNullOrWhiteSpace(roleText) || !Enum.TryParse<UserRole>(roleText, out var role))
        {
            MessageBox.Show("Rol tanlanmagan yoki noto'g'ri formatda.", "Xato",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var request = new UserRequest
        {
            Name = txtName.Text.Trim(),
            Phone = txtPhone.Text.Trim(),
            Address = txtAddress.Text.Trim(),
            Description = txtDescription.Text.Trim(),
            Role = role,
            UserName = txtUsername.Text.Trim(),
            Password = txtPassword.Password.Trim(),
            Accounts = [
                new()
                {
                    CurrencyId = (long)cbxValutaType.SelectedValue,
                    OpeningBalance = GetOpeningBalance(),
                    Discount = 0
                }
            ]
        };

        var response = await client.Users.Create(request).Handle();
        if (response.Data > 0)
        {
            ClearForm();
            LoadUsers();
            isCreatingNewUser = false; // 🔴 Yaratish rejimidan chiqish
        }
        else
        {
            MessageBox.Show("Foydalanuvchini qo'shishda xatolik.", "Xato",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool ValidateUserFields()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Foydalanuvchining Ismi majburiy maydon.", "Diqqat",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtName.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtPhone.Text))
        {
            MessageBox.Show("Foydalanuvchining Telefon raqami majburiy maydon.", "Diqqat",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtPhone.Focus();
            return false;
        }

        if (cbRole.SelectedItem == null || string.IsNullOrWhiteSpace(cbRole.SelectedItem.ToString()))
        {
            MessageBox.Show("Rol tanlanmagan.", "Diqqat",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            cbRole.Focus();
            return false;
        }

        if (cbxValutaType.SelectedValue == null || (long)cbxValutaType.SelectedValue == 0)
        {
            MessageBox.Show("Valyuta turi majburiy maydon.", "Diqqat",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            cbxValutaType.Focus();
            return false;
        }

        return true;
    }

    private void ClearForm()
    {
        txtName.Text = "";
        txtPhone.Text = "";
        txtAddress.Text = "";
        txtDescription.Text = "";
        tbDebt.Text = "";
        tbAccount.Text = "";

        // 🔴 User default qiymat
        cbRole.SelectedItem = "User";
        isCreatingNewUser = false; // 🔴 Reset
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
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
                tb.Text = "+998 ";

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

    private async void LoadingUser(long userId)
    {
        isCreatingNewUser = false;

        var exitUser = await client.Users.GetById(userId);
        currentUser = exitUser.Data;
        if (currentUser == null) return;

        dgUsers.SelectedItem = currentUser;

        // Formani to'ldirish
        cbRole.SelectedItem = currentUser.Role.ToString();
        txtName.Text = currentUser.Name;
        txtPhone.Text = currentUser.Phone;
        txtAddress.Text = currentUser.Address;
        txtDescription.Text = currentUser.Description;

        // 🔴 SHU YERDA MANTIQ O'ZGARADI:
        // 1. Tizimga kirgan odam ADMIN bo'lsa
        // 2. Tahrirlanayotgan odam HODIM bo'lsa (yoki o'sha tahrirlanayotgan odamning o'zi admin bo'lsa)
        bool isEmployee = currentUser.Role == UserRole.Hodim;
        bool isEditingAdminSelf = currentUser.UserName?.ToLower() == "admin";

        if (IsLoggedInAsAdmin && (isEmployee || isEditingAdminSelf))
        {
            pnlEmployeeAccess.Visibility = Visibility.Visible;
            txtUsername.Text = currentUser.UserName; // Bazadagi logini
            txtPassword.Password = ""; // Parolni xavfsizlik uchun bo'sh ko'rsatamiz

            // Agar logini bo'lsa, toggleni yoqib qo'yamiz
            tglHasAccess.IsChecked = !string.IsNullOrEmpty(currentUser.UserName);
        }
        else
        {
            pnlEmployeeAccess.Visibility = Visibility.Collapsed;
            tglHasAccess.IsChecked = false;
        }

        // Accountlar qismi... (o'zgarishsiz qoladi)
        if (currentUser.Accounts != null && currentUser.Accounts.Count > 0)
        {
            var account = currentUser.Accounts[0];
            cbxValutaType.SelectedValue = account.CurrencyId;
            if (account.Balance < 0) { tbDebt.Text = Math.Abs(account.Balance).ToString("N2"); tbAccount.Text = ""; }
            else if (account.Balance > 0) { tbAccount.Text = account.Balance.ToString("N2"); tbDebt.Text = ""; }
        }

        btnSave.Visibility = Visibility.Collapsed;
        btnUpdate.Visibility = Visibility.Visible;
    }
    private void btnEdit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not UserResponse user)
            return;

        var result = MessageBox.Show(
            $"{user.Name} ma'lumotlarini tahrirlamoqchimisiz?",
            "Tahrirlash",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            LoadingUser(user.Id);
        }
    }

    [RelayCommand]
    private static void RemoveUser(UserResponse response)
    {
        var vm = new SemiProductViewModel { ErrorMessage = string.Empty };
        vm.ErrorMessage = $"{response.Name}ni ma'lumotlarini o'chirmoqchimisiz?";
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
        if (!string.IsNullOrWhiteSpace(tbDebt.Text) &&
            tbDebt.Text.Trim() != "0" &&
            tbDebt.Text.Trim() != "," &&
            tbDebt.Text.Trim() != ".")
        {
            tbAccount.Text = "";
        }
    }

    private void TbAccount_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(tbAccount.Text) &&
            tbAccount.Text.Trim() != "0" &&
            tbAccount.Text.Trim() != "," &&
            tbAccount.Text.Trim() != ".")
        {
            tbDebt.Text = "";
        }
    }

    private decimal GetOpeningBalance()
    {
        string debtText = tbDebt.Text?.Trim();
        string accountText = tbAccount.Text?.Trim();

        if (!string.IsNullOrWhiteSpace(debtText) &&
            decimal.TryParse(debtText, out decimal debt) &&
            debt > 0)
        {
            return -debt;
        }

        if (!string.IsNullOrWhiteSpace(accountText) &&
            decimal.TryParse(accountText, out decimal balance) &&
            balance > 0)
        {
            return balance;
        }

        return 0;
    }

    private async void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UserResponse user = null;

            // Bir nechta usul bilan foydalanuvchini olish
            if (sender is Button btn)
            {
                // Avval Tag'dan tekshirish
                if (btn.Tag is UserResponse tagUser)
                {
                    user = tagUser;
                }
                // Keyin CommandParameter'dan tekshirish
                else if (btn.CommandParameter is UserResponse paramUser)
                {
                    user = paramUser;
                }
            }

            if (user == null)
            {
                MessageBox.Show("Foydalanuvchi tanlanmagan!", "Xato",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Tasdiqlash
            if (MessageBox.Show(
                $"{user.Name} foydalanuvchisini o'chirmoqchimisiz?",
                "O'chirishni tasdiqlash",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            // O'chirish
            var response = await client.Users.Delete(user.Id);

            if (response.IsSuccess)
            {
                MessageBox.Show("Foydalanuvchi muvaffaqiyatli o'chirildi!",
                    "Muvaffaqiyat",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Ro'yxatni yangilash
                LoadUsers();

                // Formani tozalash
                ClearForm();
                btnSave.Visibility = GetSaveButtonVisibility();
                btnUpdate.Visibility = Visibility.Collapsed;
                currentUser = null;
            }
            else
            {
                MessageBox.Show($"O'chirishda xatolik: {response.Message}",
                    "Xato",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}",
                "Xato",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}