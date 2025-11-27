using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.Pages.Reports.ViewModels;
using Forex.Wpf.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

public partial class FinishedStockReportViewModel : ViewModelBase
{
    private readonly ForexClient _client;
    private readonly CommonReportDataService _commonData;

    // Serverdan keladigan to‘liq ma’lumot
    private readonly ObservableCollection<FinishedStockItemViewModel> _allItems = [];

    // UI ga ko‘rinadigan filtrlangan ro‘yxat
    [ObservableProperty]
    private ObservableCollection<FinishedStockItemViewModel> items = [];

    public ObservableCollection<ProductViewModel> AvailableProducts => _commonData.AvailableProducts;

    [ObservableProperty] private ProductViewModel? selectedCode;
    [ObservableProperty] private ProductViewModel? selectedProduct;


    public FinishedStockReportViewModel(ForexClient client, CommonReportDataService commonData)
    {
        _client = client;
        _commonData = commonData;

        // Har qanday property o‘zgarsa avtomatik filter boladi
        this.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(SelectedCode) or nameof(SelectedProduct))
                ApplyFilters();
        };

        _ = LoadAsync();
    }

    // 🔵 Serverdan ma’lumot yuklash
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        _allItems.Clear();
        Items.Clear();

        try
        {
            var request = new FilteringRequest
            {
                Filters = new()
                {
                    ["productType"] = ["include:product.unitMeasure"],
                    ["ProductEntries"] = ["include"]
                }
            };

            var response = await _client.ProductResidues.Filter(request)
                .Handle(l => IsLoading = l);

            if (!response.IsSuccess || response.Data == null)
            {
                ErrorMessage = "Tayyor mahsulot qoldiqlari yuklanmadi";
                return;
            }

            foreach (var stock in response.Data)
            {
                var pt = stock.ProductType;
                if (pt == null) continue;

                var product = pt.Product;
                if (product == null) continue;

                decimal unitPrice = stock.ProductType.UnitPrice;

                _allItems.Add(new FinishedStockItemViewModel
                {
                    Code = product.Code ?? "-",
                    Name = product.Name ?? "-",
                    Type = pt.Type ?? "-",
                    BundleItemCount = pt.BundleItemCount,
                    TotalCount = stock.Count,
                    UnitPrice = unitPrice,
                    TotalAmount = unitPrice * stock.Count
                });
            }

            ApplyFilters();
        }
        finally { IsLoading = false; }
    }

    // 🔵 Filtrlash funksiyasi
    private void ApplyFilters()
    {
        var result = _allItems.AsEnumerable();

        if (SelectedProduct != null)
            result = result.Where(x => x.Name == SelectedProduct.Name);

        if (SelectedCode != null)
            result = result.Where(x => x.Code == SelectedCode.Code);

        Items = new ObservableCollection<FinishedStockItemViewModel>(result);
    }

    // 🔵 Tozalash
    [RelayCommand]
    private void ClearFilter()
    {
        SelectedCode = null;
        SelectedProduct = null;
        // ApplyFilters avtomatik ishlaydi
    }

    // 🔵 PRINT
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
            dlg.PrintDocument(CreateFixedDocument().DocumentPaginator, "Tayyor mahsulot qoldig‘i");
        }
    }

    // 🔵 PREVIEW
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
            Title = "Tayyor mahsulot qoldig‘i",
            Width = 1000,
            Height = 800,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Content = viewer
        };

        window.ShowDialog();
    }

    // 🔵 EXCEL EXPORT
    [RelayCommand]
    private async Task ExportToExcel()
    {
        if (!Items.Any())
        {
            MessageBox.Show("Excelga eksport qilish uchun ma'lumot yo‘q.", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Excel fayllari (*.xlsx)|*.xlsx",
            FileName = $"TayyorMahsulot_{DateTime.Today:dd.MM.yyyy}.xlsx"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Tayyor mahsulot qoldig‘i");

            int row = 1;

            ws.Cell(row, 1).Value = "TAYYOR MAHSULOT QOLDIG‘I";
            ws.Range(row, 1, row, 8).Merge().Style
                .Font.SetBold().Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            row += 2;

            ws.Cell(row, 1).Value = $"Sana: {DateTime.Today:dd.MM.yyyy}";
            ws.Range(row, 1, row, 8).Merge().Style.Font.SetFontSize(12);
            row += 2;

            // Header
            string[] headers = { "Kodi", "Nomi", "Razmer", "Donasi", "Qop soni", "Jami", "Narxi", "Umumiy" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(row, i + 1).Value = headers[i];

            ws.Range(row, 1, row, headers.Length).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);
            row++;

            // Data
            foreach (var x in Items)
            {
                ws.Cell(row, 1).Value = x.Code;
                ws.Cell(row, 2).Value = x.Name;
                ws.Cell(row, 3).Value = x.Type;
                ws.Cell(row, 4).Value = x.BundleItemCount; // Qopdagi
                ws.Cell(row, 5).Value = x.BundleCount;     // Qop soni
                ws.Cell(row, 6).Value = x.TotalCount;     // Jami
                ws.Cell(row, 7).Value = x.UnitPrice;      // Narxi
                ws.Cell(row, 8).Value = x.TotalAmount;    // Umumiy
                row++;
            }

            // Total summa
            var totalAmount = Items.Sum(i => i.TotalAmount);
            ws.Cell(row, 7).Value = "JAMI:";
            ws.Cell(row, 7).Style.Font.SetBold();
            ws.Cell(row, 8).Value = totalAmount;
            ws.Cell(row, 8).Style.Font.SetBold().NumberFormat.Format = "#,##0.00";

            ws.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);

            MessageBox.Show("Excel fayl muvaffaqiyatli saqlandi!", "Tayyor", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}");
        }
    }

    // 🔵 PDF/Print uchun document yaratish
    private FixedDocument CreateFixedDocument()
    {
        var doc = new FixedDocument();
        const double pageWidth = 794;
        const double pageHeight = 1123;

        var page = new FixedPage
        {
            Width = pageWidth,
            Height = pageHeight,
            Background = Brushes.White
        };

        var container = new StackPanel { Margin = new Thickness(40) };

        container.Children.Add(new TextBlock
        {
            Text = "TAYYOR MAHSULOT QOLDIG‘I",
            FontSize = 22,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        });

        container.Children.Add(new TextBlock
        {
            Text = $"Sana: {DateTime.Today:dd.MM.yyyy}",
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 20)
        });

        // Table
        var table = new Grid();
        double[] widths = { 70, 130, 80, 80, 120, 60, 80, 100 };
        foreach (var w in widths)
            table.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(w) });

        // Header (Qopdagi soni va Qop soni almashtirildi)
        AddRow(table, true, "Kodi", "Nomi", "Razmer", "Qop soni", "Donasi", "Jami", "Narxi", "Umumiy");

        foreach (var x in Items)
        {
            AddRow(table, false,
                x.Code,
                x.Name,
                x.Type,
                x.BundleCount!.ToString(),            // Qop soni
                x.BundleItemCount.ToString(),        // Qopdagi soni
                x.TotalCount.ToString("N0"),         // Jami
                x.UnitPrice.ToString("N2"),          // Narxi
                x.TotalAmount.ToString("N2")         // Umumiy
            );
        }

        // Total summa (Umumiy)
        var totalSum = Items.Sum(i => i.TotalAmount);
        AddRow(table, true, "JAMI:", "", "", "", "", "", "", totalSum.ToString("N2"));

        container.Children.Add(table);
        page.Children.Add(container);

        var pc = new PageContent();
        ((IAddChild)pc).AddChild(page);
        doc.Pages.Add(pc);

        return doc;
    }

    // ===== Helper =====
    private void AddRow(Grid grid, bool isHeader, params string[] values)
    {
        int row = grid.RowDefinitions.Count;
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        for (int i = 0; i < values.Length; i++)
        {
            var tb = new TextBlock
            {
                Text = values[i],
                Padding = new Thickness(4, 5, 4, 5),
                FontSize = isHeader ? 12 : 11,
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                TextAlignment = !isHeader
                    ? i switch
                    {
                        1 => TextAlignment.Left,             // Nomi chapda
                        5 or 6 or 7 => TextAlignment.Right, // Jami, Narxi, Umumiy o‘ngda
                        _ => TextAlignment.Center           // Qolgan ustunlar o‘rtada
                    }
                    : TextAlignment.Center,                  // Headerlar doimo o‘rtada
                VerticalAlignment = VerticalAlignment.Center
            };

            var border = new System.Windows.Controls.Border
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

}
