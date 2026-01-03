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
        const double pageWidth = 793.7;
        const double pageHeight = 1122.5;
        const double margin = 40;
        const double bottomReservedSpace = 80;

        var items = FilteredItems.ToList();
        if (!items.Any()) return doc;

        var totalDebtor = items.Sum(x => x.DebtorAmount);
        var totalCreditor = items.Sum(x => x.CreditorAmount);
        var totalBalance = totalDebtor - totalCreditor;

        int currentIndex = 0;
        int pageNumber = 1;
        bool totalsAdded = false;

        while (currentIndex < items.Count || !totalsAdded)
        {
            var page = new FixedPage { Width = pageWidth, Height = pageHeight, Background = Brushes.White };
            double currentTop = margin;

            // 1. SARLAVHA - SAHIFA O'RTASIDA
            if (pageNumber == 1)
            {
                var title = new TextBlock
                {
                    Text = "DEBITOR VA KREDITORLAR HISOBOTI",
                    FontSize = 22,
                    FontWeight = FontWeights.ExtraBold,
                    TextAlignment = TextAlignment.Center, // Matnni o'zini o'rtaga olish
                    Width = pageWidth - 2 * margin // Sahifa kengligi bo'yicha
                };
                FixedPage.SetLeft(title, margin);
                FixedPage.SetTop(title, currentTop);
                page.Children.Add(title);
                currentTop += 50;

                var dateInfo = new TextBlock
                {
                    Text = $"Sana: {DateTime.Today:dd.MM.yyyy}",
                    FontSize = 14,
                    Foreground = Brushes.Gray,
                    TextAlignment = TextAlignment.Center, // Sanani ham o'rtaga olish
                    Width = pageWidth - 2 * margin
                };
                FixedPage.SetLeft(dateInfo, margin);
                FixedPage.SetTop(dateInfo, currentTop);
                page.Children.Add(dateInfo);
                currentTop += 40;
            }
            else
            {
                currentTop += 20;
            }

            // 2. JADVAL
            var grid = new Grid { Width = pageWidth - 2 * margin };
            double[] widths = { 45, 150, 130, 135, 125, 125 };
            foreach (var w in widths)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(w) });

            AddRow(grid, true, "T/r", "Mijoz nomi", "Telefon", "Manzil", "Debitor", "Kreditor");

            while (currentIndex < items.Count)
            {
                var item = items[currentIndex];
                var tempGrid = new Grid { Width = grid.Width };
                foreach (var col in grid.ColumnDefinitions)
                    tempGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = col.Width });

                string deb = item.DebtorAmount > 0 ? item.DebtorAmount.ToString("N0") : "";
                string kred = item.CreditorAmount > 0 ? item.CreditorAmount.ToString("N0") : "";

                AddRow(tempGrid, false, (currentIndex + 1).ToString(), item.Name ?? "-", item.Phone ?? "-", item.Address ?? "-", deb, kred);
                tempGrid.Measure(new Size(grid.Width, double.PositiveInfinity));
                double rowHeight = tempGrid.DesiredSize.Height;

                grid.Measure(new Size(grid.Width, double.PositiveInfinity));
                if (currentTop + grid.DesiredSize.Height + rowHeight > pageHeight - bottomReservedSpace)
                    break;

                AddRow(grid, false, (currentIndex + 1).ToString(), item.Name ?? "-", item.Phone ?? "-", item.Address ?? "-", deb, kred);
                currentIndex++;
            }

            // 3. JAMI VA BALANS (OXIRGI BETDA)
            // 3. JAMI VA BALANS (OXIRGI BETDA)
            if (currentIndex == items.Count && !totalsAdded)
            {
                // Odatiy JAMI qatori (avvalgidek qolaveradi)
                AddRow(grid, true, "", "JAMI:", "", "", totalDebtor.ToString("N0"), totalCreditor.ToString("N0"));

                // --- UMUMIY BALANSNI JADVALNING OXIRGI QATORI SIFATIDA QO'SHISH ---
                string balanceValue = totalBalance >= 0 ? $"+{totalBalance:N2}" : totalBalance.ToString("N2");
                Brush balanceColor = totalBalance >= 0 ? Brushes.Green : Brushes.Red;

                // Yangi qator yaratamiz
                int lastRowIndex = grid.RowDefinitions.Count;
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // 1. Dastlabki 4 ta ustunni (T/r, Nomi, Telefon, Manzil) birlashtirib "UMUMIY BALANS" yozamiz
                var lblBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1.2, 0, 0.5, 1.2), // Pastki va chap cheti qalinroq
                    Background = Brushes.AliceBlue,
                    Padding = new Thickness(10, 8, 10, 8),
                    Child = new TextBlock
                    {
                        Text = "UMUMIY BALANS:",
                        FontSize = 16,
                        FontWeight = FontWeights.ExtraBold,
                        TextAlignment = TextAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                Grid.SetRow(lblBorder, lastRowIndex);
                Grid.SetColumn(lblBorder, 0);
                Grid.SetColumnSpan(lblBorder, 4); // 4 ta ustunni birlashtirish
                grid.Children.Add(lblBorder);

                // 2. Oxirgi 2 ta ustunni (Debitor, Kreditor) birlashtirib qiymatni yozamiz
                var valBorder = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 1.2, 1.2), // Pastki va o'ng cheti qalinroq
                    Background = Brushes.AliceBlue,
                    Padding = new Thickness(10, 8, 20, 8), // O'ngdan 20px padding (kesilib qolmasligi uchun)
                    Child = new TextBlock
                    {
                        Text = balanceValue,
                        FontSize = 16,
                        FontWeight = FontWeights.ExtraBold,
                        Foreground = balanceColor,
                        TextAlignment = TextAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                Grid.SetRow(valBorder, lastRowIndex);
                Grid.SetColumn(valBorder, 4);
                Grid.SetColumnSpan(valBorder, 2); // 2 ta ustunni birlashtirish
                grid.Children.Add(valBorder);

                totalsAdded = true;
            }
            FixedPage.SetLeft(grid, margin);
            FixedPage.SetTop(grid, currentTop);
            page.Children.Add(grid);

            // FOOTER
            var footer = new TextBlock { Text = $"{pageNumber}-bet / [total]", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = Brushes.Gray, TextAlignment = TextAlignment.Right, Width = 200 };
            FixedPage.SetLeft(footer, pageWidth - margin - 200);
            FixedPage.SetTop(footer, pageHeight - 40);
            page.Children.Add(footer);

            var pageContent = new PageContent();
            ((IAddChild)pageContent).AddChild(page);
            doc.Pages.Add(pageContent);
            pageNumber++;
        }

        UpdatePageNumbers(doc);
        return doc;
    }
    private void AddRow(Grid grid, bool isHeader, params string[] values)
    {
        int row = grid.RowDefinitions.Count;
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int i = 0; i < values.Length; i++)
        {
            TextAlignment align = isHeader
                ? TextAlignment.Center
                : i == 0 ? TextAlignment.Center
                : i >= 4 ? TextAlignment.Right
                : TextAlignment.Left;

            var tb = new TextBlock
            {
                Text = values[i],
                Padding = new Thickness(6, 4, 6, 4),
                FontSize = isHeader ? 13 : 12,
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                TextAlignment = align,
                VerticalAlignment = VerticalAlignment.Center
            };

            var border = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(isHeader ? 1.2 : 0.5),
                Background = isHeader ? Brushes.LightGray : Brushes.Transparent,
                Child = tb
            };

            Grid.SetRow(border, row);
            Grid.SetColumn(border, i);
            grid.Children.Add(border);
        }
    }

    private void UpdatePageNumbers(FixedDocument doc)
    {
        int totalPages = doc.Pages.Count;
        foreach (PageContent pc in doc.Pages)
        {
            var page = (FixedPage)pc.Child;
            foreach (var child in page.Children.OfType<TextBlock>())
            {
                if (child.Text.Contains("[total]"))
                    child.Text = child.Text.Replace("[total]", totalPages.ToString());
            }
        }
    }
    #endregion
}