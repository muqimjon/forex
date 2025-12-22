namespace Forex.Wpf.Pages.Reports.ViewModels;

using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
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

public partial class SalesHistoryReportViewModel : ViewModelBase
{
    private readonly ForexClient client;
    private readonly CommonReportDataService commonData;

    // Asosiy ma'lumotlar (serverdan 1 marta olinadi)
    private readonly ObservableCollection<SaleHistoryItemViewModel> allItems = [];

    // UI da ko‘rinadigan (filtrlangan)
    [ObservableProperty]
    private ObservableCollection<SaleHistoryItemViewModel> filteredItems = [];

    public ObservableCollection<UserViewModel> AvailableCustomers => commonData.AvailableCustomers;
    public ObservableCollection<ProductViewModel> AvailableProducts => commonData.AvailableProducts;

    [ObservableProperty] private UserViewModel? selectedCustomer;
    [ObservableProperty] private ProductViewModel? selectedProduct;
    [ObservableProperty] private ProductViewModel? selectedCode;
    [ObservableProperty] private DateTime beginDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    [ObservableProperty] private DateTime endDate = DateTime.Today;

    public SalesHistoryReportViewModel(ForexClient client, CommonReportDataService commonData)
    {
        this.client = client;
        this.commonData = commonData;

        // Har qanday filtr o‘zgarsa → darrov filtrla
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(SelectedCustomer) or nameof(SelectedProduct) or nameof(SelectedCode) or nameof(BeginDate) or nameof(EndDate))
                ApplyFilters();
        };

        _ = LoadAsync();
    }

    #region Commands

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        allItems.Clear();
        FilteredItems.Clear();

        var request = new FilteringRequest
        {
            Filters = new()
            {
                ["date"] = [$">={BeginDate:o}", $"<{EndDate.AddDays(1):o}"],
                ["customer"] = ["include"],
                ["saleItems"] = ["include:productType.product.unitMeasure"]
            }
        };

        var response = await client.Sales.Filter(request).Handle(l => IsLoading = l);

        if (!response.IsSuccess)
        {
            ErrorMessage = "Sotuvlar yuklanmadi";
            return;
        }

        foreach (var sale in response.Data)
        {
            if (sale.SaleItems == null) continue;

            foreach (var item in sale.SaleItems)
            {
                var product = item.ProductType?.Product;
                if (product == null) continue;

                allItems.Add(new SaleHistoryItemViewModel
                {
                    Date = sale.Date.ToLocalTime(),
                    Customer = sale.Customer?.Name ?? "-",
                    Code = product.Code ?? "-",
                    ProductName = product.Name ?? "-",
                    Type = item.ProductType?.Type ?? "-",
                    BundleCount = item.BundleCount,
                    BundleItemCount = item.ProductType?.BundleItemCount ?? 0,
                    TotalCount = item.TotalCount,
                    UnitMeasure = product.UnitMeasure?.Name ?? "dona",
                    UnitPrice = item.UnitPrice,
                    Amount = item.Amount
                });
            }
        }

        ApplyFilters();
    }

    [RelayCommand]
    private void ClearFilter()
    {
        SelectedCustomer = null;
        SelectedProduct = null;
        SelectedCode = null;
        BeginDate = DateTime.Today;
        EndDate = DateTime.Today;
    }

    [RelayCommand]
    private async Task Filter() => await LoadAsync();

    [RelayCommand]
    private void Preview()
    {
        if (!FilteredItems.Any())
        {
            MessageBox.Show("Ko‘rsatish uchun ma'lumot yo‘q.", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var doc = CreateFixedDocument();
        var viewer = new DocumentViewer { Document = doc, Margin = new Thickness(20) };

        var shareButton = new Button
        {
            Content = "Telegram’da ulashish",
            Margin = new Thickness(10),
            Padding = new Thickness(15, 8, 15, 8),
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        toolbar.Children.Add(shareButton);

        var layout = new DockPanel();
        DockPanel.SetDock(toolbar, Dock.Top);
        layout.Children.Add(toolbar);
        layout.Children.Add(viewer);

        var window = new Window
        {
            Title = $"Savdo tarixi • {BeginDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}",
            Width = 1000,
            Height = 800,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Content = layout
        };
        window.ShowDialog();
    }

    [RelayCommand]
    private void Print()
    {
        if (!FilteredItems.Any())
        {
            MessageBox.Show("Chop etish uchun ma’lumot yo‘q.", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dlg = new PrintDialog();
        if (dlg.ShowDialog() == true)
        {
            dlg.PrintDocument(CreateFixedDocument().DocumentPaginator, "Savdo tarixi");
        }
    }

    [RelayCommand]
    private async Task ExportToExcel()
    {
        if (!FilteredItems.Any())
        {
            MessageBox.Show("Excelga eksport qilish uchun ma'lumot yo‘q.", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Excel fayllari (*.xlsx)|*.xlsx",
            FileName = $"Savdo_tarixi_{BeginDate:dd.MM.yyyy}-{EndDate:dd.MM.yyyy}.xlsx"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Savdo tarixi");

            int row = 1;

            // Sarlavha
            ws.Cell(row, 1).Value = "SAVDO TARIXI HISOBOTI";
            ws.Range(row, 1, row, 11).Merge().Style
                .Font.SetBold().Font.SetFontSize(18)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            row += 2;

            // Davr
            ws.Cell(row, 1).Value = $"Davr: {BeginDate:dd.MM.yyyy} — {EndDate:dd.MM.yyyy}";
            ws.Range(row, 1, row, 11).Merge().Style.Font.SetBold().Font.SetFontSize(14);
            row += 2;

            // Header
            string[] headers = { "Sana", "Mijoz", "Kodi", "Nomi", "Razmer", "Qop soni", "Donasi", "Jami", "O‘lchov", "Narxi", "Umumiy summa" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(row, i + 1).Value = headers[i];
            ws.Range(row, 1, row, 11).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);
            row++;

            // Ma'lumotlar
            foreach (var item in FilteredItems)
            {
                ws.Cell(row, 1).Value = item.Date.ToString("dd.MM.yyyy");
                ws.Cell(row, 2).Value = item.Customer;
                ws.Cell(row, 3).Value = item.Code;
                ws.Cell(row, 4).Value = item.ProductName;
                ws.Cell(row, 5).Value = item.Type;
                ws.Cell(row, 6).Value = item.BundleCount;
                ws.Cell(row, 7).Value = item.BundleItemCount;
                ws.Cell(row, 8).Value = item.TotalCount;
                ws.Cell(row, 9).Value = item.UnitMeasure;
                ws.Cell(row, 10).Value = item.UnitPrice;
                ws.Cell(row, 11).Value = item.Amount;
                row++;
            }

            // Jami summa
            var totalAmount = FilteredItems.Sum(x => x.Amount);
            ws.Cell(row, 1).Value = "JAMI:";
            ws.Cell(row, 1).Style.Font.SetBold();
            ws.Cell(row, 11).Value = totalAmount;
            ws.Cell(row, 11).Style.Font.SetBold().NumberFormat.Format = "#,##0.00";

            ws.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);

            MessageBox.Show("Excel fayl muvaffaqiyatli saqlandi!", "Tayyor", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}");
        }
    }

    #endregion Commands

    #region Private Methods

    private void ApplyFilters()
    {
        var result = allItems.AsEnumerable();

        if (SelectedCustomer != null)
            result = result.Where(x => x.Customer == SelectedCustomer.Name);

        if (SelectedProduct != null)
            result = result.Where(x => x.ProductName == SelectedProduct.Name);

        if (SelectedCode != null)
            result = result.Where(x => x.Code == SelectedCode.Code);

        if (BeginDate == EndDate)
        {
            var begin = BeginDate.Date;
            var end = EndDate.Date.AddDays(1);
            result = result.Where(x => x.Date >= begin && x.Date < end);
        }
        else if (BeginDate != EndDate)
        {
            var begin = BeginDate.Date;
            var end = EndDate.Date;
            result = result.Where(x => x.Date >= begin && x.Date <= end);
        }

        FilteredItems = new ObservableCollection<SaleHistoryItemViewModel>(result);
    }


    private FixedDocument CreateFixedDocument()
    {
        var doc = new FixedDocument();

        // A4 (96 dpi) — aniq o‘lcham
        const double pageWidth = 794;
        const double pageHeight = 1123;

        var page = new FixedPage
        {
            Width = pageWidth,
            Height = pageHeight,
            Background = Brushes.White
        };

        // Asosiy konteyner — 45px har tomondan chekka
        var container = new Grid
        {
            Width = pageWidth - 90,  // 794 - 90 = 704px ish maydoni
            Margin = new Thickness(45, 40, 45, 40)
        };

        var stack = new StackPanel();

        // SARLAVHA
        stack.Children.Add(new TextBlock
        {
            Text = "SAVDO TARIXI HISOBOTI",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8),
            Foreground = Brushes.DarkBlue
        });

        stack.Children.Add(new TextBlock
        {
            Text = $"Davr: {BeginDate.ToString("dd.MM.yyyy") ?? "-"} — {(EndDate.ToString("dd.MM.yyyy") ?? "-")}",
            FontSize = 15,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 25)
        });

        // JADVAL
        var table = new Grid();

        // Ustun enlari
        double[] widths = {
        56,    // 0. Sana
        80,    // 1. Mijoz
        52,    // 2. Kodi
        60,    // 3. Nomi
        58,    // 4. Razmer
        60,    // 5. Qop soni
        60,    // 6. Donasi
        52,    // 7. Jami
        50,    // 8. O‘lchov
        70,    // 9. Narxi
        100    // 10. Umumiy summa
    };

        foreach (var w in widths)
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(w) });

        // Header
        AddRow(table, true,
            "Sana", "Mijoz", "Kodi", "Nomi", "Razmer", "Qop soni",
            "Donasi", "Jami", "O‘lchov", "Narxi", "Umumiy summa");

        // Ma'lumotlar
        foreach (var item in FilteredItems)
        {
            AddRow(table, false,
                item.Date.ToString("dd.MM.yyyy"),
                item.Customer,
                item.Code,
                item.ProductName,
                item.Type,
                item.BundleCount.ToString("N0"),
                item.BundleItemCount.ToString("N0"),
                item.TotalCount.ToString("N0"),
                item.UnitMeasure,
                item.UnitPrice.ToString("N2"),
                item.Amount.ToString("N2")
            );
        }

        // 💰 JAMI QATORNI HISOBLASH VA JOYLASHTIRISH 💰

        // Yig'indilar
        var totalBundleCount = FilteredItems.Sum(x => x.BundleCount); // Umumiy Qop soni
        var totalTotalCount = FilteredItems.Sum(x => x.TotalCount);   // Umumiy Jami (soni)
        var totalAmount = FilteredItems.Sum(x => x.Amount);           // Umumiy summa

        // JAMI qatori (Header stili qo'llanadi)
        AddRow(table, true,
            // 0-4. Birinchi 5 ustun bo'sh
            "JAMI:", "", "", "", "",
            // 5. Qop soni ustunida umumiy qop soni chiqadi (N0 formatida)
            totalBundleCount.ToString("N0"),
            // 6. Donasi ustuni bo'sh (yoki "JAMI:" uchun qulay joy)
            "",
            // 7. Jami ustunida umumiy Jami (soni) chiqadi (N0 formatida)
            totalTotalCount.ToString("N0"),
            // 8-9. O'lchov va Narxi ustunlari bo'sh
            "", "",
            // 10. Umumiy summa ustunida umumiy pul qiymati chiqadi (N2 formatida)
            $"{totalAmount:N2}"
        );

        stack.Children.Add(table);
        container.Children.Add(stack);
        page.Children.Add(container);

        var pageContent = new PageContent();
        ((IAddChild)pageContent).AddChild(page);
        doc.Pages.Add(pageContent);

        return doc;
    }
    // AddRow metodi o'zgarmaydi.
    private void AddRow(Grid grid, bool isHeader, params string[] cells)
    {
        int row = grid.RowDefinitions.Count;
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int i = 0; i < cells.Length; i++)
        {
            // 1. TextAlignment (Hizalanish) mantiqini aniqlash
            TextAlignment alignment;

            // Mijoz (1) va Nomi (3) chapda. Narxi (9) va Umumiy summa (10) o'ngda.
            if (i == 1 || i == 3) // Mijoz, Nomi
            {
                alignment = TextAlignment.Left;
            }
            else if (i == 9 || i == 10) // Narxi, Umumiy summa
            {
                alignment = TextAlignment.Right;
            }
            else // Sana, Kodi, Razmer, Qop soni, Donasi, Jami, O‘lchov
            {
                alignment = TextAlignment.Center;
            }

            var tb = new TextBlock
            {
                Text = cells[i],
                Padding = new Thickness(4, 5, 4, 5),
                FontSize = isHeader ? 11 : 10.5,
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Medium,
                TextAlignment = alignment, // <--- YANGI HIZALANISH QOIDASI
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.WrapWithOverflow
            };

            // ... (Border mantiqlari o'zgarmaydi) ...

            var border = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0.5),
                Background = isHeader ? Brushes.LightGray : Brushes.Transparent,
                Child = tb
            };

            Grid.SetRow(border, row);
            Grid.SetColumn(border, i);
            grid.Children.Add(border);
        }
    }
    //private async Task ShareAsPdfAsync(FixedDocument doc)
    //{
    //    try
    //    {
    //        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ForexReports");
    //        Directory.CreateDirectory(folder);
    //        string fileName = $"Savdo_{BeginDate:dd.MM.yyyy}-{EndDate:dd.MM.yyyy}.pdf";
    //        string path = Path.Combine(folder, fileName);

    //        // PDF saqlash (PdfSharp yoki boshqa kutubxona kerak bo‘lsa keyinroq qo‘shiladi)
    //        // Hozircha oddiy xabar
    //        MessageBox.Show($"PDF saqlandi:\n{path}\nTelegram orqali ulashing!", "Tayyor", MessageBoxButton.OK, MessageBoxImage.Information);

    //        Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true });
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show($"Ulashishda xato: {ex.Message}");
    //    }
    //}

    #endregion Private Methods
}