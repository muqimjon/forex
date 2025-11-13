namespace Forex.Wpf.Pages.Sales.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Input;

public partial class SalePageViewModel : ViewModelBase
{
    private readonly ForexClient client;
    private readonly IMapper mapper;
    public SalePageViewModel(ForexClient client, IMapper mapper)
    {
        this.client = client;
        this.mapper = mapper;

        _ = LoadDataAsync();
    }

    // 🗓 Sana
    [ObservableProperty] private DateTime operationDate = DateTime.Now;
    [ObservableProperty] private decimal? totalAmount;
    [ObservableProperty] private decimal? finalAmount;
    [ObservableProperty] private decimal? totalAmountWithUserBalance;
    [ObservableProperty] private string note = string.Empty;

    [ObservableProperty] private SaleItemViewModel currentSaleItem = new();
    [ObservableProperty] private ObservableCollection<SaleItemViewModel> saleItems = [];

    [ObservableProperty] private UserViewModel? customer;
    [ObservableProperty] private ObservableCollection<UserViewModel> availableCustomers = [];
    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];


    #region Load Data

    private async Task LoadDataAsync()
    {
        await LoadUsersAsync();
        await LoadProductsAsync();
    }

    public async Task LoadUsersAsync()
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["role"] = ["mijoz"],
                ["accounts"] = ["include:currency"]
            }
        };

        var response = await client.Users.Filter(request).Handle(isLoading => IsLoading = isLoading);
        if (response.IsSuccess)
            AvailableCustomers = mapper.Map<ObservableCollection<UserViewModel>>(response.Data!);
        else ErrorMessage = response.Message ?? "Mahsulot turlarini yuklashda xatolik.";
    }

    public async Task LoadProductsAsync()
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["ProductType"] = ["include:Product"],
                ["Count"] = [">0"]
            }
        };

        var response = await client.ProductResidues.Filter(request).Handle(isLoading => IsLoading = isLoading);

        if (!response.IsSuccess)
        {
            ErrorMessage = response.Message ?? "Mahsulotlarni yuklashda xatolik.";
            return;
        }

        var productResidues = mapper.Map<ObservableCollection<ProductResidueViewModel>>(response.Data!);

        var allTypes = productResidues.Select(pr => pr.ProductType)
            .Where(pt => pt is not null && pt.Product is not null)
            .ToList();

        var grouped = allTypes
            .GroupBy(pt => pt.Product.Id);

        var products = new ObservableCollection<ProductViewModel>();

        foreach (var group in grouped)
        {
            var sampleType = group.First();
            var product = sampleType.Product;

            product.ProductTypes = new ObservableCollection<ProductTypeViewModel>(group);

            products.Add(product);
        }

        AvailableProducts = products;
    }

    #endregion Load Data

    #region Commands

    [RelayCommand]
    private void Add()
    {
        if (CurrentSaleItem is null || CurrentSaleItem.BundleCount <= 0)
        {
            WarningMessage = "Mahsulot tanlanmagan yoki miqdor noto‘g‘ri!";
            return;
        }

        SaleItemViewModel item = new()
        {
            Product = CurrentSaleItem.Product,
            ProductType = CurrentSaleItem.ProductType,
            BundleCount = CurrentSaleItem.BundleCount,
            UnitPrice = CurrentSaleItem.UnitPrice,
            Amount = CurrentSaleItem.Amount,
            TotalCount = CurrentSaleItem.TotalCount,
        };

        item.PropertyChanged += SaleItemPropertyChanged;
        SaleItems.Add(item);

        CurrentSaleItem = new SaleItemViewModel();
        RecalculateTotals();
    }

    [RelayCommand]
    private async Task Submit()
    {
        if (SaleItems.Count == 0)
        {
            WarningMessage = "Hech qanday mahsulot kiritilmagan!";
            return;
        }

        SaleRequest request = new()
        {
            Date = OperationDate,
            CustomerId = Customer?.Id ?? 0,
            TotalAmount = FinalAmount ?? 0,
            Note = Note,
            SaleItems = [.. SaleItems.Select(item => new SaleItemRequest
            {
                ProductTypeId = item.ProductType.Id,
                BundleCount = (int)item.BundleCount!,
                UnitPrice = (decimal)item.UnitPrice!,
                Amount = (decimal)item.Amount!
            })]
        };

        var response = await client.Sales.Create(request).Handle(isLoading => IsLoading = isLoading);
        if (response.IsSuccess)
        {
            SuccessMessage = $"Savdo muvaffaqiyatli yuborildi. Mahsulotlar soni: {SaleItems.Count}";
            // Chop etish so'raladi
            var result = MessageBox.Show(
                "Savdo muvaffaqiyatli saqlandi!\n\nChop etishni xohlaysizmi?",
                "Muvaffaqiyat",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Print Preview ochiladi
                ShowPrintPreview();
            }
            Clear();
        }
        else ErrorMessage = response.Message ?? "Savdoni ro'yxatga olishda xatolik!";

        await LoadDataAsync();
    }

    private void ShowPrintPreview()
    {
        if (SaleItems == null || !SaleItems.Any())
        {
            MessageBox.Show("Ko‘rsatish uchun ma’lumot yo‘q.", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var fixedDoc = CreateFixedDocumentForPrint();

        var previewWindow = new Window
        {
            Title = "Sotuv cheki - Oldindan ko‘rish",
            Width = 1000,
            Height = 780,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            WindowStyle = WindowStyle.SingleBorderWindow,
            ResizeMode = ResizeMode.CanResizeWithGrip
        };

        var viewer = new DocumentViewer
        {
            Document = fixedDoc,
            Margin = new Thickness(8)
            // Tugma yo'q — DocumentViewer o'zida chop etish bor!
        };

        previewWindow.Content = viewer;
        previewWindow.ShowDialog();
    }
    // FixedDocument yaratish — sizniki bilan deyarli bir xil, lekin bizning DataGrid headerlari bilan
    private FixedDocument CreateFixedDocumentForPrint()
    {
        // 600 DPI uchun yuqori sifatli o'lchamlar
        double dpiScale = 96.0 / 72.0; // 96 — ekran, 72 — default, lekin biz 600 DPI uchun aniqroq qilamiz
        double pageWidth = 8.27 * 72;   // A4 = 210 mm → 8.27 dyuym
        double pageHeight = 11.69 * 72; // A4 = 297 mm → 11.69 dyuym
        double margin = 35;             // Juda toza ko'rinish uchun

        var fixedDoc = new FixedDocument();
        fixedDoc.DocumentPaginator.PageSize = new Size(pageWidth, pageHeight);

        int maxRowsPerPage = 44; // 600 DPI da juda toza sig'adi
        var items = SaleItems.ToList();
        int totalPages = (int)Math.Ceiling(items.Count / (double)maxRowsPerPage);
        int pageNumber = 0;
        int processed = 0;

        while (processed < items.Count)
        {
            pageNumber++;
            var page = new FixedPage
            {
                Width = pageWidth,
                Height = pageHeight,
                Background = Brushes.White
            };

            var grid = new Grid
            {
                Margin = new Thickness(margin, 100, margin, 90) // to'liq eniga
            };

            // Sarlavha
            var title = new TextBlock
            {
                Text = "Sotilgan mahsulotlar ro'yxati",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 5)
            };
            FixedPage.SetLeft(title, margin);
            FixedPage.SetTop(title, 25);
            page.Children.Add(title);

            // Sana va Mijoz
            var info = new TextBlock
            {
                Text = $"Mijoz: {Customer?.Name.ToUpper() ?? "Naqd"} \t\t\t\t Sana: {OperationDate:dd.MM.yyyy}",
                FontSize = 15,
                Margin = new Thickness(45, 0, 45, 20)
            };
            FixedPage.SetLeft(title, (pageWidth - 300) / 2);
            FixedPage.SetTop(info, 70);
            page.Children.Add(info);

            // Headerlar — sizniki
            string[] headers = { "Kod", "Nomi", "Razmer", "Razmer soni", "Jami soni", "Narxi", "Jami summa" };
            double[] ratios = { 1.0, 2.8, 1.2, 1.3, 1.3, 1.4, 2.0 }; // A4 eniga 100% to'ldiradi!

            for (int i = 0; i < headers.Length; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(ratios[i], GridUnitType.Star)
                });
            }

            int row = 0;

            // Header
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = CreateCell(headers[i], true, TextAlignment.Center);
                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, i);
                grid.Children.Add(cell);
            }

            // Mahsulotlar
            var pageItems = items.Skip(processed).Take(maxRowsPerPage).ToList();
            foreach (var item in pageItems)
            {
                row++;
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                string[] values =
                [
                    item.Product.Code ?? "",
                item.Product.Name ?? "",
                item.ProductType.Type ?? "",
                item.BundleCount?.ToString("N0") ?? "0",
                item.TotalCount?.ToString("N0") ?? "0",
                item.UnitPrice?.ToString("N2") ?? "0.00",
                item.Amount?.ToString("N2") ?? "0.00"
                ];

                for (int i = 0; i < values.Length; i++)
                {
                    var cell = CreateCell(values[i], false, i >= 3 ? TextAlignment.Right : TextAlignment.Left);
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, i);
                    grid.Children.Add(cell);
                }
            }

            processed += pageItems.Count;

            if (processed >= items.Count)
            {
                row += 2;
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // "Jami:" — chapda, 6 ta ustunni birlashtirib (0-5 indekslar)
                var totalLabel = CreateCell("Jami:", true, TextAlignment.Left);
                Grid.SetRow(totalLabel, row);
                Grid.SetColumn(totalLabel, 0);           // Chapdan boshlanadi
                Grid.SetColumnSpan(totalLabel, 6);        // 6 ta ustun (0,1,2,3,4,5)
                totalLabel.Padding = new Thickness(10, 0, 10, 0);
                totalLabel.Background = Brushes.Transparent;
                if (totalLabel.Child is TextBlock tblabel)
                {
                    tblabel.FontSize = 16;
                    tblabel.FontWeight = FontWeights.SemiBold;
                }
                grid.Children.Add(totalLabel);

                // Qiymat — faqat oxirgi ustunda (6-indeks)
                var totalValue = CreateCell((FinalAmount ?? 0).ToString("N0"), true, TextAlignment.Right);
                Grid.SetRow(totalValue, row);
                Grid.SetColumn(totalValue, 6);            // Oxirgi ustun
                totalValue.Background = Brushes.Transparent;
                totalValue.Padding = new Thickness(15, 2, 15, 2);
                if (totalValue.Child is TextBlock tbvalue)
                {
                    tbvalue.FontSize = 16;
                    tbvalue.FontWeight = FontWeights.ExtraBold;
                    tbvalue.Foreground = new SolidColorBrush(Color.FromRgb(0, 80, 180)); // Ko'k rang
                }
                grid.Children.Add(totalValue);
            }
            // Sahifa raqami
            var pageNum = new TextBlock
            {
                Text = $"Sahifa {pageNumber} / {totalPages}",
                FontSize = 11,
                Foreground = Brushes.Gray
            };
            FixedPage.SetRight(pageNum, margin);
            FixedPage.SetBottom(pageNum, 30);
            page.Children.Add(pageNum);

            page.Children.Add(grid);

            var pageContent = new PageContent();
            ((IAddChild)pageContent).AddChild(page);
            fixedDoc.Pages.Add(pageContent);
        }

        return fixedDoc;
    }

    // Chiroyli katakcha
    private Border CreateCell(string text, bool isHeader, TextAlignment alignment = TextAlignment.Left)
    {
        var border = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(0.5),
            Background = isHeader ? new SolidColorBrush(Color.FromRgb(235, 235, 235)) : Brushes.White,
            Padding = new Thickness(6, 5, 6, 5)
        };

        var tb = new TextBlock
        {
            Text = text,
            FontSize = isHeader ? 13 : 12,
            FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
            TextAlignment = alignment,
            VerticalAlignment = VerticalAlignment.Center
        };

        border.Child = tb;
        return border;
    }
    private void Clear()
    {
        SaleItems = [];
        Customer = null;
        TotalAmount = null;
        FinalAmount = null;
        Note = string.Empty;
        TotalAmountWithUserBalance = null;
    }

    #endregion Commands

    #region Property Changes

    partial void OnCustomerChanged(UserViewModel? value) => RecalculateTotalAmountWithUserBalance();

    private void SaleItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SaleItemViewModel.Amount))
        {
            RecalculateTotals();
        }
    }

    partial void OnFinalAmountChanged(decimal? value)
    {
        if (Customer is not null)
            TotalAmountWithUserBalance = Customer.Balance - FinalAmount;
    }

    #endregion Property Changes

    #region Private Helpers

    private void RecalculateTotals()
    {
        TotalAmount = SaleItems.Sum(x => x.Amount);
        FinalAmount = TotalAmount;
    }

    private void RecalculateTotalAmountWithUserBalance()
    {
        if (Customer is not null)
            TotalAmountWithUserBalance = Customer.Balance - TotalAmount;
    }

    #endregion Private Helpers
}