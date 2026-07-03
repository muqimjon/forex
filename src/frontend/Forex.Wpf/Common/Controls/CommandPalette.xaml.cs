namespace Forex.Wpf.Common.Controls;

using Forex.ClientService.Services;
using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Auth;
using Forex.Wpf.Pages.Barcode.Views;
using Forex.Wpf.Pages.Home;
using Forex.Wpf.Pages.Products;
using Forex.Wpf.Pages.Reports;
using Forex.Wpf.Pages.Returns.Views;
using Forex.Wpf.Pages.Sales;
using Forex.Wpf.Pages.Sales.Views;
using Forex.Wpf.Pages.Settings;
using Forex.Wpf.Pages.Supply.Views;
using Forex.Wpf.Pages.Transactions.Views;
using Forex.Wpf.Pages.Users;
using Forex.Wpf.Windows;
using Forex.Wpf.Windows.OverdueAccountsWindow;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public sealed record PaletteCommand
{
    public PackIconKind Icon { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Group { get; init; } = string.Empty;
    public Action Action { get; init; } = () => { };
}

public partial class CommandPalette : UserControl
{
    private List<PaletteCommand> all = [];
    private readonly ObservableCollection<PaletteCommand> results = [];

    public CommandPalette()
    {
        InitializeComponent();
        list.ItemsSource = results;
    }

    public void Toggle()
    {
        if (Visibility == Visibility.Visible)
            Close();
        else
            Open();
    }

    public void Open()
    {
        if (!AuthStore.Instance.IsAuthenticated)
            return;

        all = BuildCommands();
        search.Text = string.Empty;
        ApplyFilter(string.Empty);

        Visibility = Visibility.Visible;
        Dispatcher.BeginInvoke(() => { search.Focus(); Keyboard.Focus(search); });
    }

    public void Close() => Visibility = Visibility.Collapsed;

    private static MainWindow? Main => Application.Current.MainWindow as MainWindow;

    private static List<PaletteCommand> BuildCommands()
    {
        var auth = AuthStore.Instance;
        var list = new List<PaletteCommand>();

        void nav(bool can, PackIconKind icon, string title, Func<Page> page)
        {
            if (can)
                list.Add(new PaletteCommand { Icon = icon, Title = title, Group = "Bo'lim", Action = () => Main?.NavigateTo(page()) });
        }

        list.Add(new PaletteCommand { Icon = PackIconKind.HomeOutline, Title = "Bosh sahifa", Group = "Bo'lim", Action = () => Main?.NavigateTo(new HomePage()) });
        nav(auth.CanSales, PackIconKind.CartOutline, "Savdo", () => new SalePage());
        nav(auth.CanSales, PackIconKind.CartPlus, "Yangi savdo", () => new AddSalePage());
        nav(auth.CanReturns, PackIconKind.KeyboardReturn, "Qaytarish", () => new ReturnPage());
        nav(auth.CanReturns, PackIconKind.CartArrowDown, "Yangi qaytarish", () => new AddReturnPage());
        nav(auth.CanPayments, PackIconKind.CashMultiple, "To'lov", () => new TransactionPage());
        nav(auth.CanProducts, PackIconKind.ShoeSneaker, "Mahsulotlar", () => new ProductPage());
        nav(auth.CanBarcode, PackIconKind.BarcodeScan, "Barkod", () => new BarcodePage());
        nav(auth.CanSupply, PackIconKind.TruckOutline, "Ta'minot", () => new SupplyPage());
        nav(auth.CanUsers, PackIconKind.AccountGroupOutline, "Foydalanuvchilar", () => new UserPage());
        nav(auth.CanReports, PackIconKind.ChartBoxOutline, "Hisobotlar", () => new ReportsPage());
        nav(auth.CanSettings, PackIconKind.CogOutline, "Sozlamalar", () => new SettingsPage());

        if (auth.CanReports)
            list.Add(new PaletteCommand { Icon = PackIconKind.AlertCircleOutline, Title = "Muddati o'tgan qarzlar", Group = "Bo'lim", Action = () => new OverdueAccountsWindow { Owner = Application.Current.MainWindow }.ShowDialog() });

        list.Add(new PaletteCommand { Icon = PackIconKind.ThemeLightDark, Title = "Mavzuni almashtirish (kunduzgi/tungi)", Group = "Amal", Action = () => AppPreferences.Instance.DarkTheme = !AppPreferences.Instance.DarkTheme });
        list.Add(new PaletteCommand { Icon = PackIconKind.LogoutVariant, Title = "Tizimdan chiqish", Group = "Amal", Action = Logout });

        return list;
    }

    private static void Logout()
    {
        LoginPage.ClearSavedSession();
        AuthStore.Instance.Logout();
        Main?.NavigateTo(new LoginPage());
    }

    private void ApplyFilter(string query)
    {
        results.Clear();

        var items = string.IsNullOrWhiteSpace(query)
            ? all
            : all.Where(c => c.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                             || c.Group.Contains(query, StringComparison.OrdinalIgnoreCase));

        foreach (var c in items)
            results.Add(c);

        if (results.Count > 0)
            list.SelectedIndex = 0;
    }

    private void Search_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter(search.Text);

    private void Move(int delta)
    {
        if (results.Count == 0)
            return;

        int next = list.SelectedIndex + delta;
        list.SelectedIndex = Math.Clamp(next, 0, results.Count - 1);
        list.ScrollIntoView(list.SelectedItem);
    }

    private void Execute(PaletteCommand? command)
    {
        if (command is null)
            return;

        Close();
        command.Action.Invoke();
    }

    private void Root_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down:
                Move(1);
                e.Handled = true;
                break;
            case Key.Up:
                Move(-1);
                e.Handled = true;
                break;
            case Key.Enter:
                Execute(list.SelectedItem as PaletteCommand);
                e.Handled = true;
                break;
            case Key.Escape:
                Close();
                e.Handled = true;
                break;
        }
    }

    private void Item_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBoxItem { DataContext: PaletteCommand command })
            Execute(command);
    }

    private void Backdrop_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (ReferenceEquals(e.OriginalSource, Backdrop))
            Close();
    }
}
