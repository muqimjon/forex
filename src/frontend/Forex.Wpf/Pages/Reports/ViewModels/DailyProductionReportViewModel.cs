namespace Forex.Wpf.Pages.Reports.ViewModels;

using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

public partial class DailyProductionReportViewModel : ViewModelBase
{
    private readonly ForexClient _client;
    private readonly CommonReportDataService _commonData;

    [ObservableProperty]
    private ObservableCollection<ProductViewModel> availableProducts = [];

    [ObservableProperty] private ObservableCollection<ProductionItemViewModel> items = [];
    [ObservableProperty] private ProductViewModel? selectedCode;
    [ObservableProperty] private DateTime beginDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    [ObservableProperty] private DateTime endDate = DateTime.Today;

    // Yuqoridagi jami ko‘rsatkichlar
    [ObservableProperty] private decimal tayyorAmount;
    [ObservableProperty] private decimal aralashAmount;
    [ObservableProperty] private decimal evaAmount;

    public DailyProductionReportViewModel(ForexClient client, CommonReportDataService commonData)
    {
        _client = client;
        _commonData = commonData;
        _ = LoadProductsAsync();

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(BeginDate) or nameof(EndDate) or nameof(SelectedCode))
                LoadDataCommand.Execute(null);
        };

        LoadDataCommand.Execute(null);
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var response = await _client.Products.GetAllAsync();
            if (response.IsSuccess && response.Data != null)
            {
                var products = response.Data
                    .Select(p => new ProductViewModel
                    {
                        Id = p.Id,
                        Code = p.Code,
                        Name = p.Name
                    })
                    .OrderBy(p => p.Code)
                    .ToList();

                AvailableProducts = new ObservableCollection<ProductViewModel>(products);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Mahsulotlar yuklanmadi: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task LoadData()
    {
        Items.Clear();

        try
        {
            var request = new FilteringRequest
            {
                Filters = new()
                {
                    ["date"] = [$">={BeginDate}", $"<{EndDate.AddDays(1)}"],
                    ["productType"] = ["include:product"]
                }
            };

            var response = await _client.ProductEntries.Filter(request).Handle(l => IsLoading = l);
            if (!response.IsSuccess)
            {
                ErrorMessage = "Ma'lumot yuklanmadi";
                return;
            }

            var entries = response.Data;

            // Jami hisoblash uchun
            decimal tayyorSum = 0, aralashSum = 0, evaSum = 0;

            int rowNum = 1;

            foreach (var entry in entries)
            {
                // Agar kod tanlangan bo‘lsa — faqat shu mahsulotni ko‘rsat
                if (SelectedCode != null && entry.ProductType?.Product?.Code != SelectedCode.Code)
                    continue;

                var product = entry.ProductType?.Product;
                if (product == null) continue;

                int jami = entry.Count;
                int donasi = entry.BundleItemCount;
                int qopSoni = jami / donasi;

                // Jami hisobga qo‘shish
                switch (product.ProductionOrigin)
                {
                    case ProductionOrigin.Tayyor: tayyorSum += jami; break;
                    case ProductionOrigin.Aralash: aralashSum += jami; break;
                    case ProductionOrigin.Eva: evaSum += jami; break;
                }

                var vm = new ProductionItemViewModel
                {
                    RowNumber = rowNum++,
                    Date = entry.Date.ToLocalTime(),
                    Code = product.Code,
                    Name = product.Name,
                    Type = entry.ProductType?.Type ?? "-",
                    BundleCount = qopSoni,
                    BundleItemCount = donasi,
                    TotalCount = jami,
                    ProductionType = product.ProductionOrigin switch
                    {
                        ProductionOrigin.Tayyor => "Tayyor",
                        ProductionOrigin.Aralash => "Aralash",
                        ProductionOrigin.Eva => "Eva",
                        _ => "Noma'lum"
                    }
                };

                Items.Add(vm);
            }

            // Yuqoridagi jami ko‘rsatkichlarni yangilash
            TayyorAmount = tayyorSum;
            AralashAmount = aralashSum;
            EvaAmount = evaSum;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}", "Xato", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }

    [RelayCommand]
    private void ClearFilter()
    {
        SelectedCode = null;
        BeginDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        EndDate = DateTime.Today;
        LoadDataCommand.Execute(null);
    }

    // PRINT
    [RelayCommand]
    private void Print()
    {
        if (!Items.Any())
        {
            MessageBox.Show("Chop etish uchun ma’lumot yo‘q!", "Xabar", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var dlg = new PrintDialog();
        if (dlg.ShowDialog() == true)
        {
            dlg.PrintDocument(CreateFixedDocument().DocumentPaginator, "Kunlik ishlab chiqarish hisoboti");
        }
    }

    // EXCEL EXPORT
    [RelayCommand]
    private void ExportToExcel()
    {
        if (!Items.Any())
        {
            MessageBox.Show("Excelga eksport qilish uchun ma'lumot yo‘q!", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Excel fayllari (*.xlsx)|*.xlsx",
            FileName = $"KunlikIshlabChiqarish_{BeginDate:dd.MM.yyyy}_{EndDate:dd.MM.yyyy}.xlsx"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Ishlab chiqarish");

            int row = 1;

            // Sarlavha — katta va o‘rtada
            ws.Cell(row, 1).Value = "KUNLIK ISHLAB CHIQARISH HISOBOTI";
            ws.Range(row, 1, row, 9).Merge();
            ws.Cell(row, 1).Style
                .Font.SetBold()
                .Font.SetFontSize(18)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#004B87"))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Row(row).Height = 35;
            row += 2;

            // Davr — chapda
            ws.Cell(row, 1).Value = $"Davri: {BeginDate:dd.MM.yyyy} — {EndDate:dd.MM.yyyy}";
            ws.Range(row, 1, row, 9).Merge();
            ws.Cell(row, 1).Style
                .Font.SetFontSize(14)
                .Font.SetBold()
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            ws.Row(row).Height = 25;
            row += 3;

            // Headerlar
            string[] headers = { "T/r", "Sana", "Kodi", "Nomi", "Razmer", "Qop soni", "Donasi", "Jami", "Tayyorlanish usuli" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(row, i + 1).Value = headers[i];
            }
            ws.Range(row, 1, row, 9).Style
                .Font.SetBold()
                .Font.SetFontSize(12)
                .Fill.SetBackgroundColor(XLColor.LightGray)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Row(row).Height = 25;
            row++;

            // Ma'lumotlar
            foreach (var x in Items)
            {
                ws.Cell(row, 1).Value = x.RowNumber;
                ws.Cell(row, 2).Value = x.Date.ToString("dd.MM.yyyy");
                ws.Cell(row, 3).Value = x.Code;
                ws.Cell(row, 4).Value = x.Name;
                ws.Cell(row, 5).Value = x.Type;
                ws.Cell(row, 6).Value = x.BundleCount;
                ws.Cell(row, 7).Value = x.BundleItemCount;
                ws.Cell(row, 8).Value = x.TotalCount;
                ws.Cell(row, 9).Value = x.ProductionType;

                // T/r va Jami — o‘rtada
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                row++;
            }

            // Bo‘sh qator
            row++;

            // Oxirgi chiroyli jami qatori — chop etishdagidek
            ws.Cell(row, 1).Value = "JAMI:";
            ws.Cell(row, 2).Value = $"Tayyor: {TayyorAmount:N0}";
            ws.Cell(row, 4).Value = $"Aralash: {AralashAmount:N0}";
            ws.Cell(row, 7).Value = $"Eva: {EvaAmount:N0}";

            ws.Range(row, 1, row, 9).Merge();
            ws.Cell(row, 1).Value = $"JAMI:     Tayyor: {TayyorAmount:N0}     |     Aralash: {AralashAmount:N0}     |     Eva: {EvaAmount:N0}";
            ws.Cell(row, 1).Style
                .Font.SetFontSize(16)
                .Font.SetBold()
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#006400"))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            ws.Row(row).Height = 40;

            // Ustun kengliklari — chop etish bilan bir xil
            ws.Column(1).Width = 8;   // T/r
            ws.Column(2).Width = 14;  // Sana
            ws.Column(3).Width = 12;  // Kodi
            ws.Column(4).Width = 25;  // Nomi
            ws.Column(5).Width = 12;  // Razmer
            ws.Column(6).Width = 12;  // Qop soni
            ws.Column(7).Width = 12;  // Donasi
            ws.Column(8).Width = 14;  // Jami
            ws.Column(9).Width = 22;  // Tayyorlanish usuli

            workbook.SaveAs(dialog.FileName);
            MessageBox.Show("Excel fayl muvaffaqiyatli saqlandi!", "Tayyor", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}", "Xato", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // PREVIEW
    [RelayCommand]
    private void Preview()
    {
        if (!Items.Any())
        {
            MessageBox.Show("Ko‘rsatish uchun ma’lumot yo‘q!", "Xabar", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var doc = CreateFixedDocument();
        var viewer = new DocumentViewer { Document = doc, Margin = new Thickness(20) };
        var window = new Window
        {
            Title = "Kunlik ishlab chiqarish hisoboti",
            Width = 1050,
            Height = 820,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Content = viewer
        };
        window.ShowDialog();
    }

    // A4 PDF/Print hujjat — to‘liq, chiroyli, sahifalarga bo‘linadi
    // FixedDocument va AddRow metodlarini to‘liq almashtiring:

    private FixedDocument CreateFixedDocument()
    {
        var doc = new FixedDocument();
        const double pageWidth = 793.7;
        const double pageHeight = 1122.5;
        double margin = 40;

        var itemsList = Items.ToList();
        if (!itemsList.Any()) return doc;

        // 1-sahifada faqat 33 ta qator chiqadi
        const int firstPageRows = 33;
        const int otherPagesRows = 50; // 2 va undan keyingi sahifalar — 50 ta qator

        int totalPages = 1 + (int)Math.Ceiling((itemsList.Count - firstPageRows) / (double)otherPagesRows);
        if (itemsList.Count <= firstPageRows) totalPages = 1;

        for (int p = 0; p < totalPages; p++)
        {
            var page = new FixedPage
            {
                Width = pageWidth,
                Height = pageHeight,
                Background = System.Windows.Media.Brushes.White
            };

            // =================== SARLAVHA + DAVR ===================
            var headerPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(margin, 25, margin, 10)
            };

            var title = new TextBlock
            {
                Text = "KUNLIK ISHLAB CHIQARISH HISOBOTI",
                FontSize = 22,
                FontWeight = FontWeights.ExtraBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 70, 130)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(170, 10, 0, 12)
            };

            var period = new TextBlock
            {
                Text = $"Davri: {BeginDate:dd.MM.yyyy} — {EndDate:dd.MM.yyyy} | Sahifa {p + 1} / {totalPages}",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            headerPanel.Children.Add(title);
            headerPanel.Children.Add(period);
            page.Children.Add(headerPanel);

            // =================== JADVAL ===================
            var grid = new Grid
            {
                Margin = new Thickness(margin, 100, margin, 10)
            };

            double[] widths = { 40, 80, 60, 115, 70, 70, 70, 90, 120 };
            foreach (var w in widths)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(w) });

            AddRow(grid, true,
                "T/r", "Sana", "Kodi", "Nomi", "Razmer", "Qop soni", "Donasi", "Jami", "Tayyorlanish usuli");

            // Qaysi sahifadagi qatorlar sonini hisoblaymiz
            int rowsThisPage = p == 0 ? firstPageRows : otherPagesRows;
            int startIndex = p == 0 ? 0 : firstPageRows + (p - 1) * otherPagesRows;
            int count = Math.Min(rowsThisPage, itemsList.Count - startIndex);

            count = Math.Min(rowsThisPage, itemsList.Count - startIndex);

            for (int i = 0; i < count; i++)
            {
                var x = itemsList[startIndex + i];
                AddRow(grid, false,
                    (startIndex + i + 1).ToString(),
                    x.Date.ToString("dd.MM.yyyy"),
                    x.Code,
                    x.Name,
                    x.Type,
                    x.BundleCount.ToString("N0"),
                    x.BundleItemCount.ToString("N0"),
                    x.TotalCount.ToString("N0"),
                    x.ProductionType);
            }

            // =================== OXIRGI SAHIFADA JAMI ===================
            if (p == totalPages - 1)
            {
                AddRow(grid, false, "", "", "", "", "", "", "", "", "");

                var totalBorder = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(2.5),
                    Background = new SolidColorBrush(Color.FromRgb(220, 255, 220)),
                    Padding = new Thickness(30, 18, 30, 18),
                    CornerRadius = new CornerRadius(15),
                    Margin = new Thickness(0, 30, 0, 20)
                };

                var totalText = new TextBlock
                {
                    Text = $"JAMI: Tayyor: {TayyorAmount:N0} | Aralash: {AralashAmount:N0} | Eva: {EvaAmount:N0}",
                    FontSize = 18,
                    FontWeight = FontWeights.ExtraBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 110, 0)),
                    TextAlignment = TextAlignment.Center
                };

                totalBorder.Child = totalText;

                int totalRowIndex = grid.RowDefinitions.Count;
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Grid.SetRow(totalBorder, totalRowIndex);
                Grid.SetColumnSpan(totalBorder, 9);
                grid.Children.Add(totalBorder);
            }

            page.Children.Add(grid);

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
            TextAlignment alignment = isHeader
                ? TextAlignment.Center
                : i switch
                {
                    0 => TextAlignment.Right,   // T/r
                    3 => TextAlignment.Left,    // Nomi
                    7 => TextAlignment.Right,   // Jami
                    _ => TextAlignment.Center    // qolganlari o‘rtada
                };

            var tb = new TextBlock
            {
                Text = values[i],
                Padding = new Thickness(6),
                FontSize = isHeader ? 13 : 12,
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                TextAlignment = alignment,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.Black
            };

            var border = new Border
            {
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(isHeader ? 1 : 0.5),
                Background = isHeader ? System.Windows.Media.Brushes.LightGray : System.Windows.Media.Brushes.Transparent,
                Child = tb
            };

            Grid.SetRow(border, row);
            Grid.SetColumn(border, i);
            grid.Children.Add(border);
        }
    }
}