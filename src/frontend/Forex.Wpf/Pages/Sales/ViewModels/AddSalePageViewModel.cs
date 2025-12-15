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

    // Initialization state tracking
    private Task? _initializationTask;

    public AddSalePageViewModel(ForexClient client, IMapper mapper, INavigationService navigation)
    {
        this.client = client;
        this.mapper = mapper;
        this.navigation = navigation;
        CurrentSaleItem.PropertyChanged += SaleItemPropertyChanged;

        _initializationTask = LoadDataAsync();
    }

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

    [ObservableProperty] private long editingSaleId = 0;

    #region Initialization

    private async Task EnsureInitializedAsync()
    {
        if (_initializationTask is not null)
        {
            await _initializationTask;
        }
    }

    private async Task LoadDataAsync()
    {
        await Task.WhenAll(
            LoadUsersAsync(),
            LoadProductsAsync()
        );
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
        else
            ErrorMessage = response.Message ?? "Mahsulot turlarini yuklashda xatolik.";
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

        var grouped = allTypes.GroupBy(pt => pt.Product.Id);

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

    #endregion

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

        CurrentSaleItem.PropertyChanged -= SaleItemPropertyChanged;

        try
        {
            CurrentSaleItem.Product = SelectedSaleItem.Product;
            CurrentSaleItem.ProductType = SelectedSaleItem.ProductType;
            CurrentSaleItem.BundleCount = SelectedSaleItem.BundleCount;
            CurrentSaleItem.UnitPrice = SelectedSaleItem.UnitPrice;
            CurrentSaleItem.Amount = SelectedSaleItem.Amount;
            CurrentSaleItem.TotalCount = SelectedSaleItem.TotalCount;

            SaleItems.Remove(SelectedSaleItem);
            SelectedSaleItem = null;

            RecalculateTotals();
        }
        finally
        {
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
        var fixedDoc = new FixedDocument();

        // A4 o'lchamlari va marginlar
        double pageWidth = 793.7;
        double pageHeight = 1122.5;

        double marginTop = 40;
        double marginBottom = 40;
        double marginLeft = 30;
        double marginRight = 30;

        double tableWorkingWidth = pageWidth - marginLeft - marginRight;
        double titleHeight = 40;
        double dateHeight = 30;
        double rowHeight = 25;

        var items = SaleItems.ToList();

        // ******************** Yig'indi qiymatlarni hisoblash ********************
        decimal totalAmountSum = items.Sum(i => i.Amount) ?? 0;
        double totalBundleCountSum = items.Sum(i => i.BundleCount) ?? 0;
        double totalTotalCountSum = items.Sum(i => i.TotalCount) ?? 0;
        // ************************************************************************

        // Sahifalashni hisoblash
        // Bu joyda Jami qatori jadvalning ichiga kirishi uchun joy ajratish kerak
        double tableAvailableHeight =
            pageHeight - marginTop - marginBottom - titleHeight - dateHeight;

        // Ma'lumot qatorlari soni + 1 (Sarlavha qatori) + 1 (Jami qatori)
        int totalRowsWithHeaderAndFooter = items.Count + 2;

        // Har bir sahifada nechta ma'lumot qatori sig'ishi
        // maxDataRowsPerPage hozirgi qolgan joyni hisobga olgan holda
        int maxDataRowsPerPage = (int)Math.Floor((tableAvailableHeight - rowHeight) / rowHeight);
        if (maxDataRowsPerPage < 1) maxDataRowsPerPage = 1;

        // Jami qator oxirgi sahifada joylashishi uchun umumiy qatorlar sonini to'g'irlash
        int totalPages = (int)Math.Ceiling((double)(items.Count + 1) / maxDataRowsPerPage);

        int processed = 0;
        int globalRowIndex = 0;

        string[] headers = { "T/r", "Kod", "Nomi", "Razmer", "Qop soni", "Jami soni", "Narxi", "Jami summa" };

        double[] finalWidths = { 40, 70, 190, 70, 70, 80, 70, 133.7 };

        for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
        {
            var page = new FixedPage
            {
                Width = pageWidth,
                Height = pageHeight,
                Background = Brushes.White
            };

            var gridContainer = new StackPanel
            {
                Margin = new Thickness(marginLeft, marginTop, marginRight, 0) // Pastki marginni 0 qoldiramiz, chunki Sahifa raqami Footerda
            };

            // Title va Info qismlari avvalgidek qoladi...
            gridContainer.Children.Add(new TextBlock
            {
                Text = "Sotilgan mahsulotlar ro'yxati",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            });

            gridContainer.Children.Add(new TextBlock
            {
                Text = $"Mijoz: {Customer?.Name.ToUpper() ?? "Naqd"} | Sana: {Date:dd.MM.yyyy}",
                FontSize = 14,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 10)
            });


            // Asosiy ma'lumotlar Grid
            var grid = new Grid
            {
                Width = tableWorkingWidth,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            // Ustun ta'riflari
            for (int i = 0; i < headers.Length; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(finalWidths[i], GridUnitType.Pixel)
                });
            }

            int row = 0;

            // 1. Sarlavhalarni qo'shish
            AddRow(grid, true, row, headers);
            row++;

            // 2. Ma'lumotlarni qo'shish
            int count = Math.Min(maxDataRowsPerPage - 1, items.Count - processed); // Sarlavha allaqachon bir qatorni egallagan

            var pageItems = items.Skip(processed).Take(count).ToList();

            foreach (var item in pageItems)
            {
                globalRowIndex++;
                string[] values =
                [
                    globalRowIndex.ToString(),
                    item.Product.Code ?? "",
                    item.Product.Name ?? "",
                    item.ProductType.Type ?? "",
                    item.BundleCount?.ToString("N0") ?? "0",
                    item.TotalCount?.ToString("N0") ?? "0",
                    item.UnitPrice?.ToString("N2") ?? "0.00",
                    item.Amount?.ToString("N2") ?? "0.00"
                ];

                AddRow(grid, false, row, values);
                row++;
            }

            processed += pageItems.Count;

            // ******************** Jami qatorini qo'shish (faqat oxirgi sahifada) ********************
            if (pageIndex == totalPages - 1)
            {
                // Bo'sh joy
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
                row++;

                // Jami qatori uchun yangi qator
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // 1. Jami: Labelni yaratish (T/r, Kod, Nomi, Razmer ustunlarini birlashtirish)
                var totalLabel = CreateCell("Jami:", true, TextAlignment.Right);
                Grid.SetRow(totalLabel, row);
                Grid.SetColumn(totalLabel, 0);
                Grid.SetColumnSpan(totalLabel, 4); // 4 ustun birlashdi
                totalLabel.Background = Brushes.Transparent;

                // Birlashtirilgan katakning borderlari
                totalLabel.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);

                if (totalLabel.Child is TextBlock tblabel)
                {
                    tblabel.FontSize = 16;
                    tblabel.FontWeight = FontWeights.SemiBold;
                    // Paddingni o'ng tomonga siljitish
                    tblabel.Padding = new Thickness(0, 5, 10, 5);
                }
                grid.Children.Add(totalLabel);

                // 2. Umumiy Qop soni (Index 4)
                var totalBundleCountValue = CreateCell(totalBundleCountSum.ToString("N0"), true, TextAlignment.Right);
                Grid.SetRow(totalBundleCountValue, row);
                Grid.SetColumn(totalBundleCountValue, 4);
                totalBundleCountValue.Background = Brushes.Transparent;
                totalBundleCountValue.BorderThickness = new Thickness(0.5);
                grid.Children.Add(totalBundleCountValue);

                // 3. Umumiy Jami soni (Index 5)
                var totalTotalCountValue = CreateCell(totalTotalCountSum.ToString("N0"), true, TextAlignment.Right);
                Grid.SetRow(totalTotalCountValue, row);
                Grid.SetColumn(totalTotalCountValue, 5);
                totalTotalCountValue.Background = Brushes.Transparent;
                totalTotalCountValue.BorderThickness = new Thickness(0.5);
                grid.Children.Add(totalTotalCountValue);

                // 4. Narxi ustunidagi bo'sh katak (Index 6)
                var emptyCell = CreateCell("", true, TextAlignment.Center);
                Grid.SetRow(emptyCell, row);
                Grid.SetColumn(emptyCell, 6);
                emptyCell.Background = Brushes.Transparent;
                emptyCell.BorderThickness = new Thickness(0.5);
                grid.Children.Add(emptyCell);

                // 5. Umumiy Narxi (Jami summa) - Index 7
                var totalAmountValue = CreateCell(totalAmountSum.ToString("N2"), true, TextAlignment.Right);
                Grid.SetRow(totalAmountValue, row);
                Grid.SetColumn(totalAmountValue, 7);
                totalAmountValue.Background = Brushes.Transparent;
                totalAmountValue.BorderThickness = new Thickness(0.5);
                if (totalAmountValue.Child is TextBlock tbvalue)
                {
                    tbvalue.FontSize = 16;
                    tbvalue.FontWeight = FontWeights.ExtraBold;
                    tbvalue.Foreground = new SolidColorBrush(Color.FromRgb(0, 80, 180));
                }
                grid.Children.Add(totalAmountValue);
            }
            // ****************************************************************************************

            // Asosiy Gridni konteynerga qo'shish
            gridContainer.Children.Add(grid);
            page.Children.Add(gridContainer);


            // ******************** FOOTER: Sahifa raqamini joylashtirish ********************
            var pageNum = new TextBlock
            {
                Text = $"Sahifa {pageIndex + 1} / {totalPages}",
                FontSize = 11,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Right // Kerakli joyga joylashish uchun
            };

            // Sahifa raqamini FixedPage'ning pastki o'ng burchagiga joylashtirish
            FixedPage.SetRight(pageNum, marginRight);
            FixedPage.SetBottom(pageNum, marginBottom);
            page.Children.Add(pageNum);
            // ******************************************************************************

            var pc = new PageContent();
            ((IAddChild)pc).AddChild(page);
            fixedDoc.Pages.Add(pc);
        }

        return fixedDoc;
    }


    private void AddRow(Grid grid, bool isHeader, int row, params string[] values)
    {
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // T/r:0, Kod:1, Nomi:2, Razmer:3, Qop:4, Jami:5, Narxi:6, Summa:7
        TextAlignment[] alignments =
        {
            TextAlignment.Center, // 0 T/r
            TextAlignment.Left,   // 1 Kod
            TextAlignment.Left,   // 2 Nomi
            TextAlignment.Center, // 3 Razmer
            TextAlignment.Right,  // 4 Qop soni
            TextAlignment.Right,  // 5 Jami soni
            TextAlignment.Right,  // 6 Narxi
            TextAlignment.Right   // 7 Jami summa
        };

        TextAlignment finalAlignment;

        for (int i = 0; i < values.Length; i++)
        {
            if (isHeader)
            {
                finalAlignment = (i == 0 || i == 3) ? TextAlignment.Center : TextAlignment.Left;
            }
            else
            {
                finalAlignment = alignments[i];
            }

            var cell = CreateCell(values[i], isHeader, finalAlignment);

            // Jami qatorini jadval ichida joylash uchun bu qism o'chirildi
            // if (values[i] == "Jami:") { Grid.SetColumnSpan(cell, 4); }

            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, i);
            grid.Children.Add(cell);
        }
    }


    private Border CreateCell(string text, bool isHeader, TextAlignment alignment = TextAlignment.Left)
    {
        var border = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(0.5),
            Background = isHeader ? new SolidColorBrush(Color.FromRgb(235, 235, 235)) : Brushes.White,
            Padding = new Thickness(4, 5, 4, 5)
        };

        var tb = new TextBlock
        {
            Text = text,
            FontSize = isHeader ? 13 : 12,
            FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
            TextAlignment = alignment,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Jami qatoridagi 'Jami:' so'zini stilini o'rnatish
        if (text == "Jami:")
        {
            tb.FontWeight = FontWeights.SemiBold;
            tb.FontSize = 16;
            border.Background = Brushes.Transparent;

            // ❗ YANGILANISH: Chap tomonga siljitish (TextAlignment yuqorida Left ga o'rnatilgan)
            tb.TextAlignment = TextAlignment.Left;
            tb.Padding = new Thickness(10, 5, 4, 5); // Chapdan 10px joy qoldirish
        }

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

    #endregion

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

    #endregion

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

    #endregion

    #region Public Methods for External Use

    /// <summary>
    /// Loads sale data for editing. Ensures initialization is complete first.
    /// </summary>
    public async Task LoadSaleForEditAsync(long saleId)
    {
        // Ma'lumotlar yuklanishini kutamiz
        await EnsureInitializedAsync();

        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["id"] = [saleId.ToString()],
                ["saleItems"] = ["include:productType.product"]
            }
        };

        var response = await client.Sales.Filter(request).Handle(isLoading => IsLoading = isLoading);

        if (!response.IsSuccess || !response.Data.Any())
        {
            ErrorMessage = response.Message ?? "Savdoni yuklashda xatolik!";
            return;
        }

        var sale = mapper.Map<SaleViewModel>(response.Data.First());

        EditingSaleId = sale.Id;
        Date = sale.Date;
        Note = sale.Note ?? string.Empty;

        // Endi AvailableCustomers to'liq yuklangan
        var customer = AvailableCustomers.FirstOrDefault(c => c.Id == sale.CustomerId);
        if (customer is not null)
        {
            Customer = customer;
        }

        SaleItems.Clear();
        if (sale.SaleItems is not null)
        {
            foreach (var saleItem in sale.SaleItems)
            {
                var product = AvailableProducts.FirstOrDefault(p =>
                    p.Id == saleItem.ProductType?.Product?.Id);

                if (product == null && saleItem.ProductType?.Product is not null)
                {
                    product = mapper.Map<ProductViewModel>(saleItem.ProductType.Product);
                    product.ProductTypes = new ObservableCollection<ProductTypeViewModel>();
                    AvailableProducts.Add(product);
                }

                ProductTypeViewModel? productType = null;
                if (product is not null && saleItem.ProductType is not null)
                {
                    productType = product.ProductTypes?.FirstOrDefault(pt =>
                        pt.Id == saleItem.ProductType.Id);

                    if (productType == null)
                    {
                        productType = mapper.Map<ProductTypeViewModel>(saleItem.ProductType);
                        product.ProductTypes ??= [];
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
    [ObservableProperty] private string name;
    [ObservableProperty] private string photoPath;
}