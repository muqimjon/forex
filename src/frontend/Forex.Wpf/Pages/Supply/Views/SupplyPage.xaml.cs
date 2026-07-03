namespace Forex.Wpf.Pages.Supply.Views;

using Forex.ClientService.Enums;
using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Supply.ViewModels;
using Forex.Wpf.ViewModels;
using Forex.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

public partial class SupplyPage : Page
{
    private static readonly Regex NumericRegex = new(@"^[0-9]+$", RegexOptions.Compiled);
    private static MainWindow Main => (MainWindow)Application.Current.MainWindow;
    private readonly SupplyPageViewModel vm;
    private bool _userGuard;

    public SupplyPage()
    {
        InitializeComponent();
        vm = App.AppHost!.Services.GetRequiredService<SupplyPageViewModel>();
        DataContext = vm;

        Loaded += Page_Loaded;
        SetupUserComboBox();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        this.ResizeWindow(940, 640);
        ShortcutAttacher.RegisterShortcut(btnBack, Key.Escape);
        ShortcutAttacher.RegisterShortcut(btnSubmit, Key.Enter, ModifierKeys.Control);
    }

    private void SetupUserComboBox()
    {
        var combo = cbxUser.InternalComboBox;
        combo.StaysOpenOnEdit = true;

        combo.GotFocus += (_, _) =>
        {
            vm.ApplyUserFilter(null);
            combo.IsDropDownOpen = true;
        };

        combo.LostFocus += UserComboBox_LostFocus;

        void SetupEditBox()
        {
            if (combo.Template?.FindName("PART_EditableTextBox", combo) is not TextBox editBox)
                return;

            var userTyping = false;

            editBox.PreviewKeyDown += (_, e) =>
                userTyping = e.Key is not (Key.Down or Key.Up or Key.Enter or Key.Escape or Key.Tab or Key.Left or Key.Right);

            editBox.TextChanged += (_, _) =>
            {
                if (!userTyping)
                    return;

                userTyping = false;
                vm.ApplyUserFilter(editBox.Text?.Trim());
                combo.IsDropDownOpen = true;
            };
        }

        if (combo.IsLoaded)
            SetupEditBox();
        else
            combo.Loaded += (_, _) => SetupEditBox();
    }

    private void UserComboBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_userGuard)
            return;

        var text = cbxUser.Text?.Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            vm.SelectedUser = null;
            vm.ApplyUserFilter(null);
            return;
        }

        if (cbxUser.SelectedItem is UserViewModel selected &&
            selected.Name.Equals(text, StringComparison.OrdinalIgnoreCase))
        {
            vm.ApplyUserFilter(null);
            return;
        }

        var existing = vm.AvailableUsers.FirstOrDefault(u => u.Name.Equals(text, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            vm.SelectedUser = existing;
            vm.ApplyUserFilter(null);
            return;
        }

        _userGuard = true;

        var label = vm.SelectedPartyType == SupplyPartyType.Supplier ? "ta'minotchi" : "vositachi";
        var confirm = MessageBox.Show(
            $"'{text}' topilmadi. Yangi {label} yaratilsinmi?",
            "Yangi", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm == MessageBoxResult.Yes)
        {
            var created = OpenCreateUserWindow(text);
            if (created is not null)
            {
                vm.AddCreatedUser(created);
                _userGuard = false;
                return;
            }
        }

        Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
        {
            var combo = cbxUser.InternalComboBox;
            combo.Focus();
            if (combo.Template?.FindName("PART_EditableTextBox", combo) is TextBox editBox)
                editBox.SelectAll();
            _userGuard = false;
        });
    }

    private UserViewModel? OpenCreateUserWindow(string name)
    {
        var isSupplier = vm.SelectedPartyType == SupplyPartyType.Supplier;

        var window = new UserWindow
        {
            Owner = Window.GetWindow(this),
            Role = isSupplier ? UserRole.Taminotchi : UserRole.Vositachi,
            AccountCurrencyId = vm.SelectedCurrency?.Id ?? 0,
            Title = isSupplier ? "Yangi ta'minotchi qo'shish" : "Yangi vositachi qo'shish"
        };
        window.txtName.Text = name;

        return window.ShowDialog() == true ? window.user : null;
    }

    private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !NumericRegex.IsMatch(e.Text);
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        if (NavigationService?.CanGoBack == true)
            NavigationService.GoBack();
        else
            Main.NavigateTo(new HomePage());
    }
}
