namespace Forex.Wpf.Pages.Reports.ViewModels;

using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

public partial class DebtorCreditorReportViewModel : ViewModelBase
{
    private readonly ForexClient _client;
    private readonly CommonReportDataService _commonData;
    [ObservableProperty] private ObservableCollection<DebtorCreditorItemViewModel> items = [];
    [ObservableProperty] private ObservableCollection<DebtorCreditorItemViewModel> filteredItems = [];
    public ObservableCollection<UserViewModel> AvailableCustomers => _commonData.AvailableCustomers;
    [ObservableProperty] private UserViewModel? selectedCustomer;
    [ObservableProperty] private decimal totalDebtor;
    [ObservableProperty] private decimal totalCreditor;
    [ObservableProperty] private decimal totalBalance;

    public DebtorCreditorReportViewModel(ForexClient client, CommonReportDataService commonData)
    {
        _client = client;
        _commonData = commonData;
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SelectedCustomer))
                ApplyFilter();
        };
        _ = LoadAsync();
    }

    #region Load Data
    private async Task LoadAsync()
    {
        var users = await LoadUsersAsync();
        if (users == null) return;
        var mapped = MapUsersToDebtorCreditor(users);
        Items = new ObservableCollection<DebtorCreditorItemViewModel>(mapped);
        FilteredItems = new ObservableCollection<DebtorCreditorItemViewModel>(mapped);
        UpdateTotals();
    }

    private async Task<List<UserResponse>?> LoadUsersAsync()
    {
        var response = await _client.Users.GetAllAsync().Handle(l => IsLoading = l);
        if (!response.IsSuccess)
        {
            ErrorMessage = "Foydalanuvchilar yuklanmadi";
            return null;
        }
        return response.Data;
    }
    #endregion Load Data

    #region Private Helpers
    private List<DebtorCreditorItemViewModel> MapUsersToDebtorCreditor(List<UserResponse> users)
    {
        var list = new List<DebtorCreditorItemViewModel>();
        foreach (var u in users)
        {
            if (u.UserName == "admin") continue;
            var balance = u.FirstBalance ?? 0;
            list.Add(new DebtorCreditorItemViewModel
            {
                Id = u.Id,
                Name = u.Name,
                Phone = u.Phone,
                Address = u.Address,
                DebtorAmount = balance < 0 ? Math.Abs(balance) : 0,
                CreditorAmount = balance > 0 ? balance : 0
            });
        }
        return list;
    }

    private void UpdateTotals()
    {
        TotalDebtor = FilteredItems.Sum(x => x.DebtorAmount);
        TotalCreditor = FilteredItems.Sum(x => x.CreditorAmount);
        TotalBalance = TotalDebtor - TotalCreditor;
    }
    #endregion Private Helpers

    #region Filters
    private void ApplyFilter()
    {
        if (Items == null) return;
        var filtered = Items.ToList();
        if (SelectedCustomer != null)
        {
            filtered = filtered.Where(x => x.Id == SelectedCustomer.Id).ToList();
        }
        FilteredItems = new ObservableCollection<DebtorCreditorItemViewModel>(filtered);
        UpdateTotals();
    }

    #region Commands
    [RelayCommand]
    private void ClearFilter()
    {
        SelectedCustomer = null;
        FilteredItems = new ObservableCollection<DebtorCreditorItemViewModel>(Items);
        UpdateTotals();
    }

    [RelayCommand]
    private void ExportToExcel()
    {
        try
        {
            if (FilteredItems == null || !FilteredItems.Any())
            {
                MessageBox.Show("Eksport qilish uchun ma'lumot topilmadi.", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel fayllari (*.xlsx)|*.xlsx",
                FileName = "Debitor va Kreditorlar.xlsx"
            };
            if (dialog.ShowDialog() != true) return;

            var sumDebtor = FilteredItems.Sum(x => x.DebtorAmount);
            var sumCreditor = FilteredItems.Sum(x => x.CreditorAmount);
            var umumiyBalans = sumDebtor - sumCreditor;

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("DebitorKreditor");
                ws.Cell(1, 1).Value = "DEBITOR VA KREDITORLAR HISOBOTI";
                ws.Range("A1:E1").Merge();
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                string[] headers = new[] { "Mijoz", "Telefon", "Manzil", "Debitor", "Kreditor" };
                for (int i = 0; i < headers.Length; i++)
                    ws.Cell(3, i + 1).Value = headers[i];
                var headerRange = ws.Range("A3:E3");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                int row = 4;
                foreach (var item in FilteredItems)
                {
                    ws.Cell(row, 1).Value = item.Name;
                    ws.Cell(row, 2).Value = item.Phone;
                    ws.Cell(row, 3).Value = item.Address;
                    ws.Cell(row, 4).Value = item.DebtorAmount;
                    ws.Cell(row, 5).Value = item.CreditorAmount;

                    for (int col = 4; col <= 5; col++)
                        ws.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
                    row++;
                }

                int jamiRow = row;
                ws.Cell(jamiRow, 1).Value = "Jami:";
                ws.Range(jamiRow, 1, jamiRow, 3).Merge();
                ws.Cell(jamiRow, 1).Style.Font.Bold = true;

                ws.Cell(jamiRow, 4).Value = sumDebtor;
                ws.Cell(jamiRow, 5).Value = sumCreditor;
                ws.Range(jamiRow, 4, jamiRow, 5).Style.Font.Bold = true;
                ws.Range(jamiRow, 4, jamiRow, 5).Style.NumberFormat.Format = "#,##0.00";

                int umumiyRow = jamiRow + 1;
                ws.Cell(umumiyRow, 1).Value = "Umumiy balans:";
                ws.Cell(umumiyRow, 1).Style.Font.Bold = true;
                ws.Range(umumiyRow, 1, umumiyRow, 4).Merge();
                ws.Cell(umumiyRow, 5).Value = umumiyBalans;
                ws.Cell(umumiyRow, 5).Style.Font.Bold = true;
                ws.Cell(umumiyRow, 5).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(umumiyRow, 5).Style.Font.FontColor = umumiyBalans > 0
                    ? ClosedXML.Excel.XLColor.Green
                    : ClosedXML.Excel.XLColor.Red;

                ws.Columns().AdjustToContents();
                workbook.SaveAs(dialog.FileName);
            }
            MessageBox.Show("Ma'lumotlar muvaffaqiyatli eksport qilindi", "Tayyor", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}", "Xato", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Print()
    {
        if (FilteredItems == null || !FilteredItems.Any())
        {
            MessageBox.Show("Chop etish uchun ma’lumot topilmadi.", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var fixedDoc = CreateFixedDocument();
        var dlg = new PrintDialog();
        if (dlg.ShowDialog() == true)
            dlg.PrintDocument(fixedDoc.DocumentPaginator, "Debitor va Kreditorlar");
    }
    #endregion Commands

    [RelayCommand]
    private void Preview()
    {
        if (!FilteredItems.Any())
        {
            MessageBox.Show("Ko‘rsatish uchun ma’lumot yo‘q!", "Xabar", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var doc = CreateFixedDocument();
        var viewer = new DocumentViewer { Document = doc, Margin = new Thickness(20) };

        var window = new Window
        {
            Title = "Debitor va Kreditorlar hisoboti",
            Width = 1050,
            Height = 820,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Content = viewer
        };
        window.ShowDialog();
    }

    private FixedDocument CreateFixedDocument()
    {
        var doc = new FixedDocument();

        double pageWidth = 793.7;
        double pageHeight = 1122.5;

        double marginTop = 60;
        double marginBottom = 80;
        double marginLeft = 40;
        double marginRight = 40;

        double titleHeight = 50;
        double dateHeight = 35;
        double rowHeight = 28;

        var items = FilteredItems.ToList();
        if (!items.Any()) return doc;

        var totalDebtor = items.Sum(x => x.DebtorAmount);
        var totalCreditor = items.Sum(x => x.CreditorAmount);
        var totalBalance = totalDebtor - totalCreditor;

        double availableHeight = pageHeight - marginTop - marginBottom - titleHeight - dateHeight;
        int rowsPerPage = (int)(availableHeight / rowHeight);
        if (rowsPerPage < 10) rowsPerPage = 10;

        int totalPages = (int)Math.Ceiling((items.Count + 3.0) / rowsPerPage);

        // 2-sahifadan 1-sahifaga qo‘shiladigan qatorlar soni — ENDI 11 TA!
        const int extraRowsFromSecondPage = 11;

        for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
        {
            var page = new FixedPage
            {
                Width = pageWidth,
                Height = pageHeight,
                Background = Brushes.White
            };

            var container = new StackPanel
            {
                Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom)
            };

            container.Children.Add(new TextBlock
            {
                Text = "DEBITOR VA KREDITORLAR HISOBOTI",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            });

            container.Children.Add(new TextBlock
            {
                Text = $"Sana: {DateTime.Today:dd.MM.yyyy} | Sahifa {pageIndex + 1} / {totalPages}",
                FontSize = 14,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15)
            });

            var table = new Grid();
            double[] widths = { 40, 170, 130, 120, 120, 120 };
            foreach (var w in widths)
                table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(w) });

            AddRow(table, true, "T/r", "Mijoz nomi", "Telefon", "Manzil", "Debitor", "Kreditor");

            int effectiveRows = rowsPerPage;
            int extraRowsThisPage = 0;

            if (pageIndex == 0 && totalPages > 1)
            {
                effectiveRows -= 3;
                extraRowsThisPage = extraRowsFromSecondPage; // 11 ta qator qo‘shiladi
            }

            int startIndex = pageIndex == 0
                ? 0
                : (rowsPerPage - 3 + extraRowsFromSecondPage) + (pageIndex - 1) * rowsPerPage;

            int count = Math.Min(effectiveRows + extraRowsThisPage, items.Count - startIndex);

            for (int i = 0; i < count; i++)
            {
                var item = items[startIndex + i];
                int tr = startIndex + i + 1;

                string deb = item.DebtorAmount > 0 ? item.DebtorAmount.ToString("N0") : "";
                string kred = item.CreditorAmount > 0 ? item.CreditorAmount.ToString("N0") : "";

                AddRow(table, false,
                    tr.ToString(),
                    item.Name ?? "-",
                    item.Phone ?? "-",
                    item.Address ?? "-",
                    deb,
                    kred);
            }

            if (pageIndex == totalPages - 1 && items.Any())
            {
                AddRow(table, false, "", "", "", "", "", "");
                AddRow(table, true, "", "JAMI:", "", "", totalDebtor.ToString("N0"), totalCreditor.ToString("N0"));

                string balanceText = totalBalance >= 0 ? $"+{totalBalance:N0}" : totalBalance.ToString("N0");
                Brush balanceColor = totalBalance >= 0 ? Brushes.Green : Brushes.Red;

                var balanceBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1.5),
                    Background = Brushes.AliceBlue,
                    Padding = new Thickness(10)
                };
                var tbBalance = new TextBlock
                {
                    Text = $"UMUMIY BALANS: {balanceText}",
                    FontSize = 16,
                    FontWeight = FontWeights.ExtraBold,
                    Foreground = balanceColor,
                    TextAlignment = TextAlignment.Right
                };
                balanceBorder.Child = tbBalance;

                int balanceRow = table.RowDefinitions.Count;
                table.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Grid.SetRow(balanceBorder, balanceRow);
                Grid.SetColumnSpan(balanceBorder, 6);
                table.Children.Add(balanceBorder);
            }

            container.Children.Add(table);
            page.Children.Add(container);

            var pageContent = new PageContent();
            ((IAddChild)pageContent).AddChild(page);
            doc.Pages.Add(pageContent);
        }

        return doc;
    }
    private void AddRow(Grid grid, bool isHeader, params string[] values)
    {
        int row = grid.RowDefinitions.Count;
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int i = 0; i < values.Length; i++)
        {
            TextAlignment align = isHeader ? TextAlignment.Center :
                i == 0 ? TextAlignment.Center :
                i >= 4 ? TextAlignment.Right : TextAlignment.Left;

            var tb = new TextBlock
            {
                Text = values[i],
                Padding = new Thickness(6, 3, 6, 3),
                FontSize = isHeader ? 13 : 12,
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                TextAlignment = align,
                VerticalAlignment = VerticalAlignment.Center
            };

            var border = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(isHeader ? 1 : 0.5),
                Background = isHeader ? Brushes.LightGray : Brushes.Transparent,
                Child = tb
            };

            Grid.SetRow(border, row);
            Grid.SetColumn(border, i);
            grid.Children.Add(border);
        }
    }
    #endregion
}