namespace Forex.Wpf.Pages.Reports.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using PdfSharp.Drawing;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public partial class CustomerTurnoverReportViewModel : ViewModelBase
{
    private readonly ForexClient _client;
    private readonly CommonReportDataService _commonData;

    [ObservableProperty] private UserViewModel? selectedCustomer;
    [ObservableProperty] private DateTime? _beginDate = DateTime.Today;
    [ObservableProperty] private DateTime? _endDate = DateTime.Today.AddDays(1).AddMinutes(-1);

    public ObservableCollection<UserViewModel> AvailableCustomers => _commonData.AvailableCustomers;


    public ObservableCollection<TurnoversViewModel> Operations { get; } = [];
    [ObservableProperty] private TurnoversViewModel? selectedItem;

    [ObservableProperty] private decimal _beginBalance;
    [ObservableProperty] private decimal _lastBalance;
    private List<OperationRecordDto> _originalRecords = [];
    public CustomerTurnoverReportViewModel(ForexClient client, CommonReportDataService commonData)
    {
        _client = client;
        _commonData = commonData;

        this.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName is nameof(SelectedCustomer) or nameof(BeginDate) or nameof(EndDate))
                await LoadDataAsync();
        };

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (SelectedCustomer is null)
        {
            Operations.Clear();
            BeginBalance = 0;
            LastBalance = 0;
            return;
        }

        var begin = BeginDate ?? DateTime.Today.AddMonths(-1);
        var end = EndDate ?? DateTime.Today;

        Operations.Clear();

        var requset = new TurnoverRequest
        (
            UserId: SelectedCustomer.Id,
            Begin: begin,
            End: end
        );

        var response = await _client.OperationRecords
            .GetTurnover(requset)
            .Handle(l => IsLoading = l);

        if (!response.IsSuccess)
            return;


        var data = response.Data;

        _originalRecords = [.. data.OperationRecords];

        BeginBalance = data.BeginBalance;
        LastBalance = data.EndBalance;

        foreach (var op in data.OperationRecords)
        {
            decimal debit = 0;
            decimal credit = 0;

            // SOTUV → har doim Credit (chiqim)
            if (op.Type == ClientService.Enums.OperationType.Sale)
            {
                debit = -op.Amount;
            }
            // TO‘LOV → Transaction bo‘lsa → IsIncome ga qarab, bo‘lmasa Amount ga qarab
            else if (op.Type == ClientService.Enums.OperationType.Transaction)
            {
                if (op.Transaction != null)
                {
                    credit = op.Transaction.IsIncome == true ? op.Amount : 0;
                    debit = op.Transaction.IsIncome == false ? Math.Abs(op.Amount) : 0;
                }
                else
                {
                    debit = op.Amount < 0 ? op.Amount : 0;
                    credit = op.Amount > 0 ? Math.Abs(op.Amount) : 0;
                }
            }

            Operations.Add(new TurnoversViewModel
            {
                Id = op.Id,
                Date = op.Date,
                Description = op.Description ?? "Izoh yo‘q",
                Debit = debit,
                Credit = credit
            });
        }
    }

    [RelayCommand]
    private async Task Delete()
    {
        var selected = SelectedItem;

        if (selected == null)
        {
            MessageBox.Show("O‘chirish uchun qatorni tanlang.", "Diqqat", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Tanlangan operatsiyani o‘chirishni xohlaysizmi?\n\n" +
            $"{selected.Date:dd.MM.yyyy} — {selected.Description}",
            "O‘chirishni tasdiqlang",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            var originalRecord = _originalRecords.FirstOrDefault(x => x.Id == selected.Id);
            if (originalRecord == null)
            {
                MessageBox.Show("Operatsiya topilmadi.", "Xato", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool success = false;

            if (originalRecord.Type == ClientService.Enums.OperationType.Sale && originalRecord.Sale != null)
            {
                var resp = await _client.Sales.Delete(originalRecord.Sale.Id).Handle(l => IsLoading = l);
                success = resp.IsSuccess;
            }
            else if (originalRecord.Type == ClientService.Enums.OperationType.Transaction && originalRecord.Transaction != null)
            {
                var resp = await _client.Transactions.Delete(originalRecord.Transaction.Id).Handle(l => IsLoading = l);
                success = resp.IsSuccess;
            }
            else
            {
                MessageBox.Show("Operatsiya turi aniqlanmadi.", "Xato");
                return;
            }

            if (success)
            {
                Operations.Remove(selected);
                _originalRecords.Remove(originalRecord);
                await LoadDataAsync(); // qoldiqlar yangilanadi

                MessageBox.Show("Operatsiya muvaffaqiyatli o‘chirildi!", "Bajarildi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Serverda o‘chirishda xatolik.", "Xato");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Preview()
    {
        if (Operations.Count == 0)
        {
            MessageBox.Show("Ko‘rsatish uchun ma’lumot yo‘q.", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var doc = CreateFixedDocument();
        ShowPreviewWindow(doc);
    }

    [RelayCommand]
    private void Print()
    {
        if (Operations.Count == 0)
        {
            MessageBox.Show("Chop etish uchun ma’lumot yo‘q.", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var doc = CreateFixedDocument();
        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            printDialog.PrintDocument(doc.DocumentPaginator, $"Mijoz hisoboti - {SelectedCustomer?.Name}");
        }
    }

    [RelayCommand]
    private void ExportToExcel()
    {
        if (Operations.Count == 0)
        {
            MessageBox.Show("Eksport qilish uchun ma’lumot yo‘q.", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var saveDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Excel fayllari (*.xlsx)|*.xlsx",
            FileName = $"Mijoz_{SelectedCustomer?.Name.Replace(" ", "_")}_{BeginDate:dd.MM.yyyy}-{EndDate:dd.MM.yyyy}.xlsx"
        };

        if (saveDialog.ShowDialog() != true) return;

        try
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Mijoz hisoboti");

            int row = 1;

            // Sarlavha
            ws.Cell(row, 1).Value = "MIJOZ OPERATSIYALARI HISOBOTI";
            ws.Range(row, 1, row, 4).Merge().Style
                .Font.SetBold().Font.SetFontSize(16).Font.SetFontColor(ClosedXML.Excel.XLColor.FromArgb(0, 102, 204))
                .Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Center);
            row += 2;

            // Mijoz va davr
            ws.Cell(row, 1).Value = $"Mijoz: {SelectedCustomer?.Name.ToUpper()}";
            ws.Cell(row, 1).Style.Font.SetBold().Font.SetFontSize(14);
            row++;
            ws.Cell(row, 1).Value = $"Davr: {BeginDate:dd.MM.yyyy} — {EndDate:dd.MM.yyyy}";
            ws.Cell(row, 1).Style.Font.SetFontSize(13);
            row += 2;

            // Header
            string[] headers = { "Sana", "Chiqim", "Kirim", "Izoh" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(row, i + 1).Value = headers[i];
                ws.Cell(row, i + 1).Style.Font.SetBold().Font.SetFontSize(13)
                    .Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Center)
                    .Fill.SetBackgroundColor(ClosedXML.Excel.XLColor.FromArgb(240, 248, 255));
            }
            row++;

            // Boshlang‘ich qoldiq
            ws.Cell(row, 1).Value = "Boshlang‘ich qoldiq";
            ws.Range(row, 1, row, 3).Merge().Style
                .Font.SetBold().Font.SetFontSize(14)
                .Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 4).Value = BeginBalance.ToString("N2");
            ws.Cell(row, 4).Style.Font.SetBold().Font.SetFontSize(15).Font.SetFontColor(ClosedXML.Excel.XLColor.DarkBlue)
                .Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Right);
            row++;

            // Operatsiyalar
            foreach (var op in Operations)
            {
                ws.Cell(row, 1).Value = op.Date.ToString("dd.MM.yyyy");
                ws.Cell(row, 1).Style.Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Center);

                if (op.Debit > 0)
                    ws.Cell(row, 2).Value = op.Debit.ToString("N0");
                if (op.Credit > 0)
                    ws.Cell(row, 3).Value = op.Credit.ToString("N0");

                ws.Cell(row, 4).Value = op.Description;

                ws.Cell(row, 2).Style.Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Right);
                ws.Cell(row, 3).Style.Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Right);
                ws.Cell(row, 4).Style.Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Left);

                row++;
            }

            // Jami
            var totalDebit = Operations.Sum(x => x.Debit);
            var totalCredit = Operations.Sum(x => x.Credit);
            ws.Cell(row, 1).Value = "JAMI";
            ws.Cell(row, 1).Style.Font.SetBold();
            if (totalDebit > 0) ws.Cell(row, 2).Value = totalDebit.ToString("N0");
            if (totalCredit > 0) ws.Cell(row, 3).Value = totalCredit.ToString("N0");
            ws.Range(row, 1, row, 4).Style.Fill.SetBackgroundColor(ClosedXML.Excel.XLColor.LightGray);
            row++;

            // Oxirgi qoldiq
            ws.Cell(row, 1).Value = "Oxirgi qoldiq";
            ws.Range(row, 1, row, 3).Merge().Style
                .Font.SetBold().Font.SetFontSize(15)
                .Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Center);
            ws.Cell(row, 4).Value = LastBalance.ToString("N2");
            ws.Cell(row, 4).Style.Font.SetBold().Font.SetFontSize(18)
                .Font.SetFontColor(LastBalance >= 0 ? ClosedXML.Excel.XLColor.DarkGreen : ClosedXML.Excel.XLColor.DarkRed)
                .Alignment.SetHorizontal(ClosedXML.Excel.XLAlignmentHorizontalValues.Right);

            // Avto kenglik
            ws.Columns().AdjustToContents();

            workbook.SaveAs(saveDialog.FileName);
            MessageBox.Show("Excel fayl muvaffaqiyatli saqlandi!", "Tayyor", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Excel yaratishda xatolik: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ClearFilter()
    {
        SelectedCustomer = null;
        BeginDate = DateTime.Today.AddMonths(-1);
        EndDate = DateTime.Today;
        Operations.Clear();
        BeginBalance = 0;
        LastBalance = 0;
    }

    private void ShowPreviewWindow(FixedDocument doc)
    {
        var viewer = new DocumentViewer { Document = doc, Margin = new Thickness(15) };

        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(10)
        };

        var shareButton = new Button
        {
            Content = "Telegram’da ulashish",
            Padding = new Thickness(15, 8, 15, 8),
            Background = new SolidColorBrush(Color.FromRgb(0, 136, 204)),
            Foreground = Brushes.White,
            FontSize = 14,
            Cursor = Cursors.Hand
        };

        shareButton.Click += (s, e) =>
        {
            try
            {
                if (SelectedCustomer == null) return;

                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string folder = Path.Combine(docs, "Forex");
                Directory.CreateDirectory(folder);

                string fileName = $"Mijoz_{SelectedCustomer.Name.Replace(" ", "_")}_{BeginDate:dd.MM.yyyy}-{EndDate:dd.MM.yyyy}.pdf";
                string path = Path.Combine(folder, fileName);

                SaveFixedDocumentToPdf(doc, path, 96);

                if (File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true });
                    Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ulashishda xatolik: {ex.Message}");
            }
        };

        toolbar.Children.Add(shareButton);

        var layout = new DockPanel();
        DockPanel.SetDock(toolbar, Dock.Top);
        layout.Children.Add(toolbar);
        layout.Children.Add(viewer);

        new Window
        {
            Title = "Mijoz aylanma hisoboti - Ko‘rish",
            Width = 1000,
            Height = 800,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Content = layout,
            Icon = Application.Current.MainWindow?.Icon
        }.ShowDialog();
    }
    private FixedDocument CreateFixedDocument()
    {
        var doc = new FixedDocument();
        var page = new FixedPage
        {
            Width = 793.7,  // A4 @ 96dpi
            Height = 1122.5,
            Background = Brushes.White
        };

        var stack = new StackPanel { Margin = new Thickness(50) };

        // Sarlavha
        stack.Children.Add(new TextBlock
        {
            Text = "MIJOZ OPERATSIYALARI HISOBOTI",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 25),
            Foreground = new SolidColorBrush(Color.FromRgb(0, 102, 204))
        });

        // Mijoz va davr
        stack.Children.Add(new TextBlock
        {
            Text = $"Mijoz: {SelectedCustomer?.Name.ToUpper() ?? "TANLANMAGAN"}",
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8)
        });

        stack.Children.Add(new TextBlock
        {
            Text = $"Davr: {(BeginDate?.ToString("dd.MM.yyyy") ?? "-")} — {(EndDate?.ToString("dd.MM.yyyy") ?? "-")}",
            FontSize = 15,
            Margin = new Thickness(0, 0, 0, 30)
        });

        double[] colWidths = { 90, 120, 120, 370 };

        // Header
        stack.Children.Add(CreateRow(colWidths, true, "Sana", "Chiqim", "Kirim", "Izoh"));

        // Boshlang‘ich qoldiq
        stack.Children.Add(CreateBalanceRow(colWidths, "Boshlang‘ich qoldiq", BeginBalance.ToString("N2")));

        // Operatsiyalar
        foreach (var op in Operations)
        {
            string debit = op.Debit > 0 ? op.Debit.ToString("N0") : "";
            string credit = op.Credit > 0 ? op.Credit.ToString("N0") : "";
            stack.Children.Add(CreateRow(colWidths, false,
                op.Date.ToString("dd.MM.yyyy"),
                debit,
                credit,
                op.Description
            ));
        }

        // Jami
        var totalDebit = Operations.Sum(x => x.Debit);
        var totalCredit = Operations.Sum(x => x.Credit);
        stack.Children.Add(CreateRow(colWidths, true, "JAMI",
            totalDebit > 0 ? totalDebit.ToString("N0") : "",
            totalCredit > 0 ? totalCredit.ToString("N0") : "",
            ""));

        // Oxirgi qoldiq
        stack.Children.Add(CreateBalanceRow(colWidths, "Oxirgi qoldiq", LastBalance.ToString("N2")));

        page.Children.Add(stack);

        var pageContent = new PageContent();
        ((IAddChild)pageContent).AddChild(page);
        doc.Pages.Add(pageContent);

        return doc;
    }
    private Grid CreateRow(double[] widths, bool isHeader, params string[] cells)
    {
        var grid = new Grid();

        for (int i = 0; i < widths.Length; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(widths[i]) });

        for (int i = 0; i < cells.Length; i++)
        {
            var tb = new TextBlock
            {
                Text = cells[i],
                Padding = new Thickness(8, 8, 8, 8),
                FontSize = isHeader ? 14 : 12,
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            // Header bo‘lsa — hammasi o‘rtada
            if (isHeader)
            {
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                tb.TextAlignment = TextAlignment.Center;
            }
            else
            {
                // Oddiy qatorlarda:
                switch (i)
                {
                    case 0: // Sana
                        tb.HorizontalAlignment = HorizontalAlignment.Center;
                        tb.TextAlignment = TextAlignment.Center;
                        break;
                    case 1: // Kirim
                    case 2: // Chiqim
                        tb.HorizontalAlignment = HorizontalAlignment.Right;   // o‘ngga
                        tb.TextAlignment = TextAlignment.Right;
                        tb.Margin = new Thickness(0, 0, 15, 0); // biroz ichkariga suramiz
                        break;
                    case 3: // Izoh
                        tb.HorizontalAlignment = HorizontalAlignment.Left;    // chapga
                        tb.TextAlignment = TextAlignment.Left;
                        tb.Margin = new Thickness(10, 0, 0, 0);
                        break;
                }
            }

            var border = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Background = isHeader ? new SolidColorBrush(Color.FromRgb(240, 248, 255)) : Brushes.White,
                Child = tb
            };

            Grid.SetColumn(border, i);
            grid.Children.Add(border);
        }

        return grid;
    }
    private Grid CreateBalanceRow(double[] widths, string label, string value)
    {
        var grid = new Grid { Margin = new Thickness(0, 10, 0, 10) };

        for (int i = 0; i < widths.Length; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(widths[i]) });

        // 1. Label — 1-2-3 ustunni birlashtirib, o‘rtada
        var labelTb = new TextBlock
        {
            Text = label,
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Foreground = Brushes.Navy
        };

        var labelBorder = new Border
        {
            BorderBrush = Brushes.DarkBlue,
            BorderThickness = new Thickness(1.5),
            Background = new SolidColorBrush(Color.FromRgb(230, 240, 255)),
            Child = labelTb
        };

        Grid.SetColumn(labelBorder, 0);
        Grid.SetColumnSpan(labelBorder, 3);
        grid.Children.Add(labelBorder);

        // 2. Qiymat — faqat 4-ustunda, o‘ngga surilgan, lekin ustun ichida markazda
        var valueTb = new TextBlock
        {
            Text = value,
            FontSize = 18,
            FontWeight = FontWeights.ExtraBold,
            Padding = new Thickness(0, 8, 20, 8),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,   // o‘ngga surish
            Foreground = label.Contains("Oxirgi")
                ? (LastBalance >= 0 ? Brushes.DarkGreen : Brushes.DarkRed)
                : Brushes.DarkBlue
        };

        var valueBorder = new Border
        {
            BorderBrush = Brushes.DarkBlue,
            BorderThickness = new Thickness(1.5),
            Background = Brushes.White,
            Child = valueTb
        };

        Grid.SetColumn(valueBorder, 3);
        grid.Children.Add(valueBorder);

        return grid;
    }
    private void SaveFixedDocumentToPdf(FixedDocument doc, string path, int dpi = 96)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);

            using var pdfDoc = new PdfSharp.Pdf.PdfDocument();
            foreach (var pageContent in doc.Pages)
            {
                var fixedPage = pageContent.GetPageRoot(false);
                if (fixedPage == null) continue;

                fixedPage.Measure(new Size(fixedPage.Width, fixedPage.Height));
                fixedPage.Arrange(new Rect(0, 0, fixedPage.Width, fixedPage.Height));
                fixedPage.UpdateLayout();

                double scale = dpi / 96.0;
                var bitmap = new RenderTargetBitmap(
                    (int)(fixedPage.Width * scale),
                    (int)(fixedPage.Height * scale),
                    dpi, dpi, PixelFormats.Pbgra32);

                var dv = new DrawingVisual();
                using (var dc = dv.RenderOpen())
                {
                    dc.PushTransform(new ScaleTransform(scale, scale));
                    dc.DrawRectangle(new VisualBrush(fixedPage), null,
                        new Rect(0, 0, fixedPage.Width, fixedPage.Height));
                }
                bitmap.Render(dv);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using var ms = new MemoryStream();
                encoder.Save(ms);
                ms.Position = 0;

                var pdfPage = pdfDoc.AddPage();
                pdfPage.Width = XUnit.FromMillimeter(210);
                pdfPage.Height = XUnit.FromMillimeter(297);

                using var xgfx = XGraphics.FromPdfPage(pdfPage);
                using var ximg = XImage.FromStream(ms);
                double ratio = Math.Min(pdfPage.Width.Point / ximg.PointWidth, pdfPage.Height.Point / ximg.PointHeight);
                double w = ximg.PointWidth * ratio;
                double h = ximg.PointHeight * ratio;
                xgfx.DrawImage(ximg, (pdfPage.Width.Point - w) / 2, (pdfPage.Height.Point - h) / 2, w, h);
            }
            pdfDoc.Save(path);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"PDF saqlashda xatolik: {ex.Message}");
        }
    }
}
