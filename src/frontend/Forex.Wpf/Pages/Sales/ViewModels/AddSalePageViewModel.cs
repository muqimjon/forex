namespace Forex.Wpf.Pages.Sales.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Common.Interfaces;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

public partial class AddSalePageViewModel : ViewModelBase
{
    private readonly ForexClient client;
    private readonly IMapper mapper;
    private readonly INavigationService navigation;

    public AddSalePageViewModel(ForexClient client, IMapper mapper, INavigationService navigation)
    {
        this.client = client;
        this.mapper = mapper;
        this.navigation = navigation;
        CurrentSaleItem.PropertyChanged += SaleItemPropertyChanged;
        _ = LoadDataAsync();
    }

    // 🗓 Sana
    [ObservableProperty] private DateTime date = DateTime.Now;
    [ObservableProperty] private decimal? totalAmount;
    [ObservableProperty] private decimal? finalAmount;
    [ObservableProperty] private decimal? totalAmountWithUserBalance;
    [ObservableProperty] private string note = string.Empty;

    [ObservableProperty] private SaleItemViewModel currentSaleItem = new();
    [ObservableProperty] private ObservableCollection<SaleItemViewModel> saleItems = [];
    [ObservableProperty] private SaleItemViewModel? selectedSaleItem = default;

    [ObservableProperty] private UserViewModel? customer;
    [ObservableProperty] private ObservableCollection<UserViewModel> availableCustomers = [];
    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];

    // Edit rejimi uchun
    [ObservableProperty] private long editingSaleId = 0;

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
        if (CurrentSaleItem.Product is null ||
            CurrentSaleItem.BundleCount == null ||
            CurrentSaleItem.ProductType is null ||
            CurrentSaleItem.UnitPrice is null)
        {
            WarningMessage = "Mahsulot tanlanmagan yoki miqdor noto'g'ri!";
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

        ClearCurrentSaleItem();
        RecalculateTotals();
    }

    [RelayCommand]
    private void Edit()
    {
        if (SelectedSaleItem is null)
            return;

        bool hasCurrentData = CurrentSaleItem.Product is not null ||
                             CurrentSaleItem.BundleCount.HasValue;

        if (hasCurrentData)
        {
            var result = MessageBox.Show(
                "Hozirgi kiritilgan ma'lumotlar o'chib ketadi. Davom etmoqchimisiz?",
                "Ogohlantirish",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
                return;
        }

        // PropertyChanged event'ni vaqtincha o'chirish
        CurrentSaleItem.PropertyChanged -= SaleItemPropertyChanged;

        try
        {
            // Ma'lumotlarni ko'chirish
            CurrentSaleItem.Product = SelectedSaleItem.Product;
            CurrentSaleItem.ProductType = SelectedSaleItem.ProductType;
            CurrentSaleItem.BundleCount = SelectedSaleItem.BundleCount;
            CurrentSaleItem.UnitPrice = SelectedSaleItem.UnitPrice;
            CurrentSaleItem.Amount = SelectedSaleItem.Amount;
            CurrentSaleItem.TotalCount = SelectedSaleItem.TotalCount;

            // DataGrid'dan olib tashlash
            SaleItems.Remove(SelectedSaleItem);
            SelectedSaleItem = null;

            // Totalni qayta hisoblash
            RecalculateTotals();
        }
        finally
        {
            // PropertyChanged event'ni qayta ulash
            CurrentSaleItem.PropertyChanged += SaleItemPropertyChanged;
        }
    }

    [RelayCommand]
    private void DeleteItem(SaleItemViewModel item)
    {
        if (item is null)
            return;

        var result = MessageBox.Show(
            $"Mahsulotni o'chirishni tasdiqlaysizmi?",
            "O'chirishni tasdiqlash",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.No)
            return;

        item.PropertyChanged -= SaleItemPropertyChanged;
        SaleItems.Remove(item);
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
            Date = Date,
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

        bool isSuccess;

        if (EditingSaleId > 0)
        {
            request.Id = EditingSaleId;
            var response = await client.Sales.Update(request).Handle(isLoading => IsLoading = isLoading);

            if (isSuccess = response.IsSuccess)
                SuccessMessage = "Savdo muvaffaqiyatli yangilandi!";
            else ErrorMessage = response.Message ?? "Savdoni yangilashda xatolik!";
        }
        else
        {
            var response = await client.Sales.Create(request).Handle(isLoading => IsLoading = isLoading);

            if (isSuccess = response.IsSuccess)
            {
                SuccessMessage = $"Savdo muvaffaqiyatli yuborildi. Mahsulotlar soni: {SaleItems.Count}";

                var result = MessageBox.Show(
                    "Savdo muvaffaqiyatli saqlandi!\n\nChop etishni xohlaysizmi?",
                    "Muvaffaqiyat",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    ShowPrintPreview();
            }
            else ErrorMessage = response.Message ?? "Savdoni ro'yxatga olishda xatolik!";
        }

        if (isSuccess)
        {
            Clear();
            navigation.GoBack();
        }
    }

    private void ShowPrintPreview()
    {
        if (SaleItems == null || !SaleItems.Any())
        {
            MessageBox.Show("Ko'rsatish uchun ma'lumot yo'q.", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var fixedDoc = CreateFixedDocumentForPrint();

        var previewWindow = new Window
        {
            Title = "Sotuv cheki - Oldindan ko'rish",
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
        };

        previewWindow.Content = viewer;
        previewWindow.ShowDialog();
    }

    private FixedDocument CreateFixedDocumentForPrint()
    {
        double pageWidth = 8.27 * 72;
        double pageHeight = 11.69 * 72;
        double margin = 35;

        var fixedDoc = new FixedDocument();
        fixedDoc.DocumentPaginator.PageSize = new Size(pageWidth, pageHeight);

        int maxRowsPerPage = 44;
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
                Margin = new Thickness(margin, 100, margin, 90)
            };

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

            var info = new TextBlock
            {
                Text = $"Mijoz: {Customer?.Name.ToUpper() ?? "Naqd"} \t\t\t\t Sana: {Date:dd.MM.yyyy}",
                FontSize = 15,
                Margin = new Thickness(45, 0, 45, 20)
            };
            FixedPage.SetLeft(title, (pageWidth - 300) / 2);
            FixedPage.SetTop(info, 70);
            page.Children.Add(info);

            string[] headers = { "Kod", "Nomi", "Razmer", "Qop soni", "Jami soni", "Narxi", "Jami summa" };
            double[] ratios = { 1.0, 2.8, 1.2, 1.3, 1.3, 1.4, 2.0 };

            for (int i = 0; i < headers.Length; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(ratios[i], GridUnitType.Star)
                });
            }

            int row = 0;

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = CreateCell(headers[i], true, TextAlignment.Center);
                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, i);
                grid.Children.Add(cell);
            }

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

                var totalLabel = CreateCell("Jami:", true, TextAlignment.Left);
                Grid.SetRow(totalLabel, row);
                Grid.SetColumn(totalLabel, 0);
                Grid.SetColumnSpan(totalLabel, 6);
                totalLabel.Padding = new Thickness(10, 0, 10, 0);
                totalLabel.Background = Brushes.Transparent;
                if (totalLabel.Child is TextBlock tblabel)
                {
                    tblabel.FontSize = 16;
                    tblabel.FontWeight = FontWeights.SemiBold;
                }
                grid.Children.Add(totalLabel);

                var totalValue = CreateCell((FinalAmount ?? 0).ToString("N0"), true, TextAlignment.Right);
                Grid.SetRow(totalValue, row);
                Grid.SetColumn(totalValue, 6);
                totalValue.Background = Brushes.Transparent;
                totalValue.Padding = new Thickness(15, 2, 15, 2);
                if (totalValue.Child is TextBlock tbvalue)
                {
                    tbvalue.FontSize = 16;
                    tbvalue.FontWeight = FontWeights.ExtraBold;
                    tbvalue.Foreground = new SolidColorBrush(Color.FromRgb(0, 80, 180));
                }
                grid.Children.Add(totalValue);
            }

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
        SaleItems.Clear();
        Customer = null;
        TotalAmount = null;
        FinalAmount = null;
        Note = string.Empty;
        TotalAmountWithUserBalance = null;
        EditingSaleId = 0;
        ClearCurrentSaleItem();
    }

    private void ClearCurrentSaleItem()
    {
        CurrentSaleItem.PropertyChanged -= SaleItemPropertyChanged;
        CurrentSaleItem = new SaleItemViewModel();
        CurrentSaleItem.PropertyChanged += SaleItemPropertyChanged;
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

    partial void OnEditingSaleIdChanged(long value)
    {
        IsEditing = value > 0;
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

    #region Public Methods for External Use

    /// <summary>
    /// Backend'dan Sale ma'lumotlarini yuklash va Edit rejimiga o'tkazish
    /// </summary>
    public async Task LoadSaleForEdit(long saleId)
    {
        // Backend'dan to'liq ma'lumotlarni olish
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["id"] = [saleId.ToString()],
                ["customer"] = ["include:accounts.currency"],
                ["saleItems"] = ["include:productType.product"]
            }
        };

        var response = await client.Sales.Filter(request).Handle(isLoading => IsLoading = isLoading);

        if (!response.IsSuccess || response.Data == null || !response.Data.Any())
        {
            ErrorMessage = response.Message ?? "Savdoni yuklashda xatolik!";
            return;
        }

        var sale = mapper.Map<SaleViewModel>(response.Data.First());

        // Formani to'ldirish
        EditingSaleId = sale.Id;
        Date = sale.Date;
        Note = sale.Note ?? string.Empty;

        // Mijozni topish va tanlash
        var customer = AvailableCustomers.FirstOrDefault(c => c.Id == sale.Customer?.Id);
        if (customer != null)
        {
            Customer = customer;
        }
        else if (sale.Customer != null)
        {
            // Agar mijoz listda bo'lmasa, qo'shish
            var newCustomer = mapper.Map<UserViewModel>(sale.Customer);
            AvailableCustomers.Add(newCustomer);
            Customer = newCustomer;
        }

        // SaleItems'ni to'ldirish
        SaleItems.Clear();
        if (sale.SaleItems != null)
        {
            foreach (var saleItem in sale.SaleItems)
            {
                // Product va ProductType'ni topish yoki yaratish
                var product = AvailableProducts.FirstOrDefault(p =>
                    p.Id == saleItem.ProductType?.Product?.Id);

                if (product == null && saleItem.ProductType?.Product != null)
                {
                    // Yangi product yaratish
                    product = mapper.Map<ProductViewModel>(saleItem.ProductType.Product);
                    product.ProductTypes = new ObservableCollection<ProductTypeViewModel>();
                    AvailableProducts.Add(product);
                }

                ProductTypeViewModel? productType = null;
                if (product != null && saleItem.ProductType != null)
                {
                    productType = product.ProductTypes?.FirstOrDefault(pt =>
                        pt.Id == saleItem.ProductType.Id);

                    if (productType == null)
                    {
                        // Yangi ProductType yaratish
                        productType = mapper.Map<ProductTypeViewModel>(saleItem.ProductType);
                        product.ProductTypes ??= new ObservableCollection<ProductTypeViewModel>();
                        product.ProductTypes.Add(productType);
                    }
                }

                var item = new SaleItemViewModel
                {
                    Product = product!,
                    ProductType = productType!,
                    BundleCount = saleItem.BundleCount,
                    UnitPrice = saleItem.UnitPrice,
                    Amount = saleItem.Amount,
                    TotalCount = saleItem.TotalCount
                };

                item.PropertyChanged += SaleItemPropertyChanged;
                SaleItems.Add(item);
            }
        }

        RecalculateTotals();
        SuccessMessage = "Savdo tahrirlash uchun yuklandi.";
    }

    #endregion


    [ObservableProperty]
    private ObservableCollection<ComboItemModel> countries =
    [
        new ComboItemModel { Name = "Uzbekistan", PhotoPath = "https://flagsapi.com/UZ/flat/64.png" },
        new ComboItemModel { Name = "USA", PhotoPath = "https://flagsapi.com/US/flat/64.png" },
        new ComboItemModel { Name = "Japan", PhotoPath = "https://flagsapi.com/JP/flat/64.png" }
    ];

    [ObservableProperty] private ComboItemModel selectedCountry;
}
public partial class ComboItemModel : ObservableObject
{
    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string photoPath;
}
