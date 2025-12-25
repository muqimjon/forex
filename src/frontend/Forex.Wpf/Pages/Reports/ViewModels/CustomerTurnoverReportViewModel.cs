namespace Forex.Wpf.Pages.Reports.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using PdfSharp.Drawing;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
    [ObservableProperty] private DateTime beginDate = DateTime.Today;
    [ObservableProperty] private DateTime endDate = DateTime.Today;

    public ObservableCollection<UserViewModel> AvailableCustomers => _commonData.AvailableCustomers;


    public ObservableCollection<TurnoversViewModel> Operations { get; } = [];
    [ObservableProperty] private TurnoversViewModel? selectedItem;

    [ObservableProperty] private decimal _beginBalance;
    [ObservableProperty] private decimal _lastBalance;

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


    #region Load Data

    private async Task LoadDataAsync()
    {
        if (SelectedCustomer is null)
        {
            Operations.Clear();
            BeginBalance = 0;
            LastBalance = 0;
            return;
        }

        Operations.Clear();

        var requset = new TurnoverRequest
        (
            UserId: SelectedCustomer.Id,
            Begin: BeginDate,
            End: EndDate
        );

        var response = await _client.OperationRecords
            .GetTurnover(requset)
            .Handle(l => IsLoading = l);

        if (!response.IsSuccess)
            return;


        var data = response.Data;


        BeginBalance = data.BeginBalance;
        LastBalance = data.EndBalance;

        foreach (var op in data.OperationRecords.OrderBy(o => o.Date)) 
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
                Date = op.Date.ToLocalTime(),
                Description = op.Description ?? "Izoh yo‘q",
                Debit = debit,
                Credit = credit
            });
        }
    }

    #endregion Load Data


    #region Commands

    [RelayCommand]
    private async Task OnTabSelectedAsync()
    {
        // Agar mijoz tanlangan bo'lsa va ma'lumotlar hali yuklanmagan bo'lsa (yoki yangilash kerak bo'lsa)
        if (SelectedCustomer != null)
        {
            await LoadDataAsync();
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

    #endregion Commands

    #region Private Helpers

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
            Padding = new Thickness(15, 2, 15, 2),
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
        var doc = new FixedDocument(); // Shu ye
        // ... (Konstantalar va boshlang'ich hisob-kitoblar o'zgarmadi) ...
        // Konstanta qiymatlar
        const double pageWidth = 793.7;
        const double pageHeight = 1122.5;
        const double margin = 50;
        const double workingWidth = pageWidth - 2 * margin;
        const double initialHeaderHeight = 120;
        const double footerHeight = 25;
        const double balanceRowHeight = 45;
        const double headerHeight = 45;
        const double totalGridHeight = 45;
        const double lastBalanceHeight = 45;
        const double minFinalSpace = 20; // JAMI va Qoldiq qatorlari orasidagi minimal bo'shliq

        // Ustun kengliklari
        double[] finalColWidths = { 80, 100, 100, 413.7 }; // Jami 693.7

        var allOperations = Operations.ToList();
        int currentIndex = 0;
        int pageNumber = 1;

        // Jami debit/kreditni bir marta hisoblab olamiz
        var totalDebit = Operations.Sum(x => x.Debit);
        var totalCredit = Operations.Sum(x => x.Credit);

        // Yagona ma'lumot qatorining minimal balandligi (taxminan 45px)
        // JAMI qatori uchun ham shu balandlik ishlatiladi
        const double requiredSpaceForFinalRows = totalGridHeight + lastBalanceHeight + 2 * minFinalSpace;


        // Har bir sahifani yaratish uchun tsikl
        while (currentIndex < allOperations.Count || pageNumber == 1)
        {
            var page = new FixedPage { Width = pageWidth, Height = pageHeight, Background = Brushes.White };
            var container = new StackPanel { Margin = new Thickness(margin) };
            double currentY = 0;
            bool isFirstPage = (pageNumber == 1);

            // 1. HEADER (Sarlavha, Mijoz, Davr)
            if (isFirstPage)
            {
                container.Children.Add(CreateTitleAndInfo(SelectedCustomer?.Name, BeginDate, EndDate));
                currentY += initialHeaderHeight;
            }

            // 2. Boshlang'ich qoldiq (Faqat 1-sahifada)
            if (isFirstPage)
            {
                var initialBalanceGrid = CreateBalanceRow(finalColWidths, "Boshlang‘ich qoldiq", BeginBalance.ToString("N2"));
                container.Children.Add(initialBalanceGrid);
                currentY += balanceRowHeight;
            }

            // 3. Header qatori (Har bir sahifada)
            var headerGrid = CreateRow(finalColWidths, true, "Sana", "Chiqim", "Kirim", "Izoh");
            container.Children.Add(headerGrid);
            currentY += headerHeight;

            // Sahifada joriy ma'lumot qatorlari uchun mavjud balandlik
            double availableHeight = pageHeight - 2 * margin - currentY - footerHeight;

            int rowsAddedOnPage = 0;

            while (currentIndex < allOperations.Count)
            {
                var op = allOperations[currentIndex];

                // Operatsiya qatorini yaratish (balandligini aniqlash uchun)
                string debit = op.Debit > 0 ? op.Debit.ToString("N0") : "";
                string credit = op.Credit > 0 ? op.Credit.ToString("N0") : "";

                var operationRowGrid = CreateRow(finalColWidths, false,
                    op.Date.ToString("dd.MM.yyyy"),
                    debit,
                    credit,
                    op.Description
                );

                // Qatorning haqiqiy balandligini o'lchash
                operationRowGrid.Measure(new Size(workingWidth, double.MaxValue));
                operationRowGrid.Arrange(new Rect(0, 0, workingWidth, operationRowGrid.DesiredSize.Height));
                double requiredHeight = operationRowGrid.DesiredSize.Height;

                // 🛑 ENG MUHIM QISM: OXIRGI QATORLARGA JOY QOLDIRISHNI TEKSHIRISH
                bool isLastOperation = (currentIndex == allOperations.Count - 1);

                if (isLastOperation)
                {
                    // Agar bu oxirgi operatsiya bo'lsa, JAMI va Qoldiq uchun joy yetarlimi tekshiramiz.
                    double neededSpace = requiredHeight + requiredSpaceForFinalRows;

                    if (neededSpace > availableHeight && rowsAddedOnPage > 0)
                    {
                        // Oxirgi operatsiya va oxirgi qoldiqlar sig'masa,
                        // ushbu operatsiyani keyingi sahifaga qoldiramiz.
                        break;
                    }
                }
                // Agar operatsiya qatori o'zi ham sig'masa, keyingi sahifaga o'tamiz
                else if (requiredHeight > availableHeight && rowsAddedOnPage > 0)
                {
                    break;
                }


                // Operatsiya qatorini qo'shish
                container.Children.Add(operationRowGrid);
                currentY += requiredHeight;
                availableHeight -= requiredHeight;
                currentIndex++;
                rowsAddedOnPage++;
            }

            // 4. Jami va Oxirgi Qoldiq (Faqat oxirgi sahifada)
            bool isLastPage = (currentIndex >= allOperations.Count);

            // Ma'lumot tsikli tugagan bo'lsa (yoki birinchi sahifada ma'lumot bo'lmasa)
            if (isLastPage)
            {
                // JAMI
                // Endi biz JAMI qatori uchun joy borligini avvaldan ta'minladik, shuning uchun shartsiz qo'shamiz
                var totalGrid = CreateRow(finalColWidths, true, "JAMI",
                    totalDebit > 0 ? totalDebit.ToString("N0") : "",
                    totalCredit > 0 ? totalCredit.ToString("N0") : "",
                    "");
                container.Children.Add(totalGrid);

                // Oxirgi Qoldiq
                // Oldingi JAMI qator qo'shilgani va yetarli joy borligi uchun, buni ham shartsiz qo'shamiz
                var lastBalanceGrid = CreateBalanceRow(finalColWidths, "Oxirgi qoldiq", LastBalance.ToString("N2"));
                container.Children.Add(lastBalanceGrid);
            }

            // Agar birinchi sahifada ma'lumot bo'lmasa (allOperations.Count == 0), uni ham qoldiqlar bilan saqlashimiz kerak.
            if (allOperations.Count == 0 && isFirstPage)
            {
                // 0 operatsiya bo'lsa, Jami 0, Oxirgi qoldiq = Boshlang'ich qoldiq bo'ladi.

                // JAMI 0
                var totalGrid = CreateRow(finalColWidths, true, "JAMI", "", "", "");
                container.Children.Add(totalGrid);

                // Oxirgi Qoldiq (Boshlang'ich qoldiqqa teng)
                var lastBalanceGrid = CreateBalanceRow(finalColWidths, "Oxirgi qoldiq", BeginBalance.ToString("N2"));
                container.Children.Add(lastBalanceGrid);
            }


            // Sahifani qo'shish va Footer mantiqlari
            page.Children.Add(container);

            // Footer elementlari (Endi AddPageFooter emas, UpdatePageFooter ishlatiladi)
            // AddPageFooter bu joyda ishlatilmasligi kerak, UpdatePageFooter tsikldan keyin ishlaydi
            // AddPageFooter(page, pageNumber, 0); // Bu qatorni o'chiramiz/izohlaymiz

            var pc = new PageContent();
            ((IAddChild)pc).AddChild(page);
            doc.Pages.Add(pc);

            pageNumber++;

            // Ma'lumot yo'q bo'lsa va bu birinchi sahifa bo'lsa, tsikldan chiqish
            if (allOperations.Count == 0 && isFirstPage) break;
        }

        // ... (Footerlarni yakuniy to'g'rilash mantiqi o'zgarmadi, bu juda muhim)
        int totalPages = doc.Pages.Count;
        for (int i = 0; i < totalPages; i++)
        {
            var p = (FixedPage)((PageContent)doc.Pages[i]).GetPageRoot(false);
            if (p != null)
            {
                UpdatePageFooter(p, i + 1, totalPages);
            }
        }

        return doc;
    }
    private StackPanel CreateTitleAndInfo(string customerName, DateTime? beginDate, DateTime? endDate)
    {
        var stack = new StackPanel();

        // Sarlavha
        stack.Children.Add(new TextBlock
        {
            Text = "MIJOZ OPERATSIYALARI HISOBOTI",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20),
            Foreground = new SolidColorBrush(Color.FromRgb(0, 102, 204))
        });

        // Mijoz va davr
        stack.Children.Add(new TextBlock
        {
            Text = $"Mijoz: {customerName?.ToUpper() ?? "TANLANMAGAN"}",
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 8)
        });

        stack.Children.Add(new TextBlock
        {
            Text = $"Davr: {(beginDate?.ToString("dd.MM.yyyy") ?? "-")} — {(endDate?.ToString("dd.MM.yyyy") ?? "-")}",
            FontSize = 15,
            Margin = new Thickness(0, 0, 0, 10)
        });

        return stack;
    }

    private double CalculateRowHeight(double commentColumnWidth, string description)
    {
        if (string.IsNullOrEmpty(description)) return 45; // Minimal balandlik (Padding 8 * 2 + Font 12 = ~45)

        // Taxminiy o'lchash mantiqi (Sizning oldingi kodingizdan soddalashtirilgan)
        var tempTextBlock = new TextBlock
        {
            Text = description,
            Width = commentColumnWidth - 20, // Padding 8*2 = 16 (taxminan 20)
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Padding = new Thickness(8),
        };

        tempTextBlock.Measure(new Size(commentColumnWidth, double.MaxValue));

        double actualHeight = tempTextBlock.DesiredSize.Height;

        // Minimal balandlikni qaytarish (45: Padding 8,8 + Font size 12)
        return Math.Max(45, actualHeight);
    }

    private void AddPageFooter(FixedPage page, int currentPage, int totalPages)
    {
        // Pastki o'ng qismga sahifalash ma'lumotini qo'shamiz (Faqat o'rnatish)
        var pageInfo = new TextBlock
        {
            Text = $"{currentPage}-bet / {totalPages}",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
        };

        FixedPage.SetRight(pageInfo, 50);
        FixedPage.SetBottom(pageInfo, 20);

        page.Children.Add(pageInfo);
    }

    private void UpdatePageFooter(FixedPage page, int currentPage, int totalPages)
    {
        const double margin = 50;

        // Avvalgi PageInfo elementini topishga harakat qilish (agar bor bo'lsa)
        // FixedPage.Children ni aylanib chiqish va topish.
        TextBlock existingPageInfo = null;
        foreach (var child in page.Children.OfType<TextBlock>())
        {
            // FixedPage.SetRight/SetBottom orqali joylashganligini tekshirishning ishonchli usuli yo'q,
            // shuning uchun biz uni o'chirib, yangidan qo'shamiz.
            if (FixedPage.GetRight(child) == margin)
            {
                existingPageInfo = child;
                break;
            }
        }

        if (existingPageInfo != null)
        {
            page.Children.Remove(existingPageInfo);
        }

        // Yangi Footer yaratish va joylashtirish
        var pageInfo = new TextBlock
        {
            Text = $"{currentPage}-bet / {totalPages}",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
        };

        FixedPage.SetRight(pageInfo, margin); // O'ng chetidan margin masofada
        FixedPage.SetBottom(pageInfo, 20);    // Pastki chetidan 20 piksel yuqorida

        page.Children.Add(pageInfo);
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

    private void SaveFixedDocumentToPdf(FixedDocument doc, string path, int dpi = 600) // ❗ DPI 300 qilib o'zgartirildi
    {
        try
        {
            // Agar fayl mavjud bo'lsa, uni o'chirish
            if (File.Exists(path)) File.Delete(path);

            using var pdfDoc = new PdfSharp.Pdf.PdfDocument();

            // Har bir FixedPage ni PDF sahifasiga o'tkazish
            foreach (var pageContent in doc.Pages)
            {
                var fixedPage = pageContent.GetPageRoot(false);
                if (fixedPage == null) continue;

                // 1. FixedPage Layout-ni yangilash
                // O'lchash (Measure) va joylashtirish (Arrange) orqali UI elementlarining haqiqiy o'lchamlarini olish
                fixedPage.Measure(new Size(fixedPage.Width, fixedPage.Height));
                fixedPage.Arrange(new Rect(0, 0, fixedPage.Width, fixedPage.Height));
                fixedPage.UpdateLayout();

                // 2. FixedPage-ni yuqori sifatli rasm (RenderTargetBitmap) ga render qilish

                // Koeffitsient (96 DPI ga nisbatan necha marta kattaroq)
                double scale = dpi / 96.0;

                var bitmap = new RenderTargetBitmap(
                    // Render qilinadigan rasmni piksel o'lchamlari
                    (int)(fixedPage.Width * scale),
                    (int)(fixedPage.Height * scale),
                    // DPI
                    dpi, dpi,
                    PixelFormats.Pbgra32);

                var dv = new DrawingVisual();
                using (var dc = dv.RenderOpen())
                {
                    // Render qilishda scaling (masshtablash) qo'llash
                    dc.PushTransform(new ScaleTransform(scale, scale));
                    dc.DrawRectangle(new VisualBrush(fixedPage), null,
                        new Rect(0, 0, fixedPage.Width, fixedPage.Height));
                }
                bitmap.Render(dv);

                // 3. Rasmni PNG stream orqali MemoryStream ga saqlash
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using var ms = new MemoryStream();
                encoder.Save(ms);
                ms.Position = 0;

                // 4. PdfSharp yordamida PDF sahifasini yaratish
                var pdfPage = pdfDoc.AddPage();
                // A4 o'lchamlarini mm da o'rnatish
                pdfPage.Width = XUnit.FromMillimeter(210);
                pdfPage.Height = XUnit.FromMillimeter(297);

                // 5. Rasmni PDF sahifasiga joylashtirish
                using var xgfx = XGraphics.FromPdfPage(pdfPage);
                using var ximg = XImage.FromStream(ms);

                // Rasm va sahifa o'lchamlari nisbatini hisoblash
                double ratio = Math.Min(pdfPage.Width.Point / ximg.PointWidth, pdfPage.Height.Point / ximg.PointHeight);
                double w = ximg.PointWidth * ratio;
                double h = ximg.PointHeight * ratio;

                // Rasmni PDF sahifasining markaziga joylashtirish
                xgfx.DrawImage(ximg, (pdfPage.Width.Point - w) / 2, (pdfPage.Height.Point - h) / 2, w, h);
            }

            // PDF faylni saqlash
            pdfDoc.Save(path);
        }
        catch (Exception ex)
        {
            // Xatolik haqida xabar berish
            MessageBox.Show($"PDF saqlashda xatolik: {ex.Message}", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion Private Helpers
}
