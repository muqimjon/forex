namespace Forex.Wpf.Pages.Returns.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Common.Interfaces;
using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.Pages.Sales.ViewModels;
using Forex.Wpf.ViewModels;
using Forex.Wpf.Windows;
using MapsterMapper;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;

public partial class AddReturnPageViewModel : ViewModelBase
{
    private readonly ForexClient client;
    private readonly IMapper mapper;
    private readonly INavigationService navigation;
    private readonly ReturnSessionService saleSession;
    private static readonly Dictionary<string, BitmapSource> _imageCache = [];
    private static readonly HttpClient _httpClient = new();

    private readonly Task? _initializationTask;

    // Hujjatning zamonaviy rang palitrasi (savdo hujjati bilan bir xil — slate)
    private static readonly Color DocInk = Color.FromRgb(0x1E, 0x29, 0x3B);
    private static readonly Color DocMuted = Color.FromRgb(0x64, 0x74, 0x8B);
    private static readonly Color DocLine = Color.FromRgb(0xD2, 0xDA, 0xE5);
    private static readonly Color DocHeaderBg = Color.FromRgb(0xF1, 0xF5, 0xF9);
    private static readonly Color DocHeaderFg = Color.FromRgb(0x47, 0x55, 0x69);
    private static readonly Color DocAccent = Color.FromRgb(0x25, 0x63, 0xEB);
    private static readonly Color DocZebra = Color.FromRgb(0xFA, 0xFB, 0xFC);

    public AddReturnPageViewModel(ForexClient client, IMapper mapper, INavigationService navigation, ReturnSessionService saleSession)
    {
        this.client = client;
        this.mapper = mapper;
        this.navigation = navigation;
        this.saleSession = saleSession;

        SaleItems = saleSession.CartItems;

        if (saleSession.TotalAmount.HasValue) TotalAmount = saleSession.TotalAmount;
        if (saleSession.FinalAmount.HasValue) FinalAmount = saleSession.FinalAmount;
        if (saleSession.Date.HasValue) Date = saleSession.Date.Value;
        if (!string.IsNullOrEmpty(saleSession.Note)) Note = saleSession.Note;
        if (saleSession.SelectedCustomer != null) Customer = saleSession.SelectedCustomer;

        CurrentSaleItem.PropertyChanged += SaleItemPropertyChanged;
        SaleItems.CollectionChanged += (s, e) => RecalculateTotals();

        RecalculateTotals();

        _initializationTask = LoadDataAsync();
    }

    [ObservableProperty] private DateTime date = DateTime.Now;
    [ObservableProperty] private decimal? totalAmount;
    [ObservableProperty] private decimal? finalAmount;
    [ObservableProperty] private decimal? totalAmountWithUserBalance;
    [ObservableProperty] private decimal? exchangeRate;
    [ObservableProperty] private string note = string.Empty;

    public bool IsForeignCurrency =>
        Customer?.CurrencyCode is { Length: > 0 } code && !code.Equals("UZS", StringComparison.OrdinalIgnoreCase);

    // Statistik ma'lumotlar
    [ObservableProperty] private int totalRowsCount;
    [ObservableProperty] private int distinctProductsCount;
    [ObservableProperty] private int totalBundlesCount;
    [ObservableProperty] private int totalQuantityCount;

    [ObservableProperty] private SaleItemViewModel currentSaleItem = new();
    [ObservableProperty] private ObservableCollection<SaleItemViewModel> saleItems = [];
    [ObservableProperty] private SaleItemViewModel? selectedSaleItem = default;

    [ObservableProperty] private ObservableCollection<string> returnUnits = ["Qop", "To'plam", "Dona"];
    [ObservableProperty] private string selectedReturnUnit = "Qop";

    [ObservableProperty] private UserViewModel? customer;
    [ObservableProperty] private string customerInput = string.Empty;
    [ObservableProperty] private ObservableCollection<UserViewModel> availableCustomers = [];
    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];

    // Filtrlanuvchi ro'yxatlar — XAML shu ikki xususiyatga bind qiladi
    [ObservableProperty] private ObservableCollection<UserViewModel> filteredCustomers = [];
    [ObservableProperty] private ICollectionView? filteredProducts;
    [ObservableProperty] private string productSearchText = string.Empty;


    [ObservableProperty] private long editingSaleId = 0;
    [ObservableProperty] private bool isEditingItem;
    [ObservableProperty] private int originalItemIndex = -1;
    private SaleItemViewModel? _editingItemSnapshot;

    #region Property changes

    partial void OnProductSearchTextChanged(string value)
    {
        // Navigatsiya paytida filterni buzmaslik uchun:
        if (CurrentSaleItem?.Product != null)
        {
            var p = CurrentSaleItem.Product;
            // Agar matn maxsulot kodi yoki nomiga teng bo'lsa, demak bu tanlov (navigatsiya) natijasi
            if (string.Equals(value, p.Code, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, p.Name, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }
        FilteredProducts?.Refresh();
    }

    // ─────────────────────────────────────────────
    // AvailableProducts / AvailableCustomers o'zgarganda filterni yangilaymiz
    // ─────────────────────────────────────────────

    partial void OnAvailableProductsChanged(ObservableCollection<ProductViewModel> value)
    {
        if (value is null) return;
        FilteredProducts = CollectionViewSource.GetDefaultView(value);
        FilteredProducts.Filter = FilterProducts;
    }

    partial void OnAvailableCustomersChanged(ObservableCollection<UserViewModel> value)
    {
        FilteredCustomers = value;
    }



    // ─────────────────────────────────────────────
    // Property Changes
    // ─────────────────────────────────────────────

    private void SaleItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SaleItemViewModel.Amount))
            RecalculateTotals();
        else if (e.PropertyName == nameof(SaleItemViewModel.ProductType))
        {
            ConvertCurrentItemPrice();
            // Razmer o'zgarsa, entry-row "Jami soni" ni tanlangan birlik bo'yicha qayta hisoblaymiz.
            if (ReferenceEquals(sender, CurrentSaleItem) && !IsEditingItem)
                RecalcCurrentItemTotal();
        }
        // Entry-row: miqdor kiritilganda "Jami soni" = miqdor × birlikdagi juft (Qop/To'plam/Dona bo'yicha).
        // SaleItemViewModel'ning qop-asosli avto-hisobini shu yerda tuzatamiz.
        else if (e.PropertyName == nameof(SaleItemViewModel.BundleCount)
                 && ReferenceEquals(sender, CurrentSaleItem) && !IsEditingItem)
        {
            RecalcCurrentItemTotal();
        }
    }

    // Entry-row uchun tanlangan birlik (Qop/To'plam/Dona) bo'yicha juft sonini hisoblaydi.
    private void RecalcCurrentItemTotal()
    {
        var type = CurrentSaleItem.ProductType;
        if (type is null) return;

        var pairsPerUnit = SelectedReturnUnit switch
        {
            "To'plam" => type.PackItemCount ?? 0,
            "Dona" => 1,
            _ => type.BundleItemCount ?? 0
        };

        CurrentSaleItem.TotalCount = (CurrentSaleItem.BundleCount ?? 0) * pairsPerUnit;
    }

    partial void OnSelectedReturnUnitChanged(string value)
    {
        if (!IsEditingItem)
            RecalcCurrentItemTotal();
    }

    partial void OnExchangeRateChanged(decimal? value)
    {
        ConvertCurrentItemPrice();
        ReconvertAllItems();
    }

    private decimal? ConvertSomToCustomer(decimal? somPrice)
    {
        if (somPrice is not > 0)
            return null;

        return IsForeignCurrency && ExchangeRate is > 0
            ? Math.Round(somPrice.Value / ExchangeRate.Value, 2)
            : somPrice;
    }

    private void ConvertCurrentItemPrice()
    {
        var converted = ConvertSomToCustomer(CurrentSaleItem.ProductType?.UnitPrice);
        if (converted is not null)
            CurrentSaleItem.UnitPrice = converted;
    }

    private void ReconvertAllItems()
    {
        foreach (var item in SaleItems)
        {
            var converted = ConvertSomToCustomer(item.ProductType?.UnitPrice);
            if (converted is not null)
                item.UnitPrice = converted;
        }
    }

    partial void OnEditingSaleIdChanged(long value)
    {
        IsEditing = value > 0;
    }

    partial void OnTotalAmountChanged(decimal? value)
    {
        if (EditingSaleId == 0) saleSession.TotalAmount = value;
    }

    partial void OnNoteChanged(string value)
    {
        if (EditingSaleId == 0) saleSession.Note = value;
    }

    partial void OnDateChanged(DateTime value)
    {
        if (EditingSaleId == 0) saleSession.Date = value;
    }

    partial void OnFinalAmountChanged(decimal? value)
    {
        if (EditingSaleId == 0) saleSession.FinalAmount = value;
        if (Customer is not null)
            TotalAmountWithUserBalance = Customer.Balance + FinalAmount;
    }

    partial void OnCustomerChanged(UserViewModel? value)
    {
        if (EditingSaleId == 0) saleSession.SelectedCustomer = value;
        CustomerInput = value?.Name ?? string.Empty;
        OnPropertyChanged(nameof(IsForeignCurrency));
        ExchangeRate = IsForeignCurrency && value?.SettlementCurrencyRate is > 0
            ? value.SettlementCurrencyRate
            : null;
        ConvertCurrentItemPrice();
        ReconvertAllItems();
        RecalculateTotalAmountWithUserBalance();
    }

    #endregion Property changes

    #region Private helpers

    private bool FilterProducts(object item)
    {
        if (string.IsNullOrWhiteSpace(ProductSearchText)) return true;
        if (item is not ProductViewModel p) return false;

        var search = ProductSearchText.Trim();
        return TransliterationHelper.ContainsIgnoreScript(p.Name ?? string.Empty, search) ||
               TransliterationHelper.ContainsIgnoreScript(p.Code ?? string.Empty, search);
    }

    // ─────────────────────────────────────────────
    // Filter public methodlar — code-behind chaqiradi
    // ─────────────────────────────────────────────

    /// <summary>
    /// Mahsulotlarni Kod yoki Nomi bo'yicha filtrlaydi.
    /// null yoki bo'sh qiymat berilsa to'liq ro'yxat ko'rsatiladi.
    /// </summary>
    public void ApplyProductFilter(string? searchText)
    {
        ProductSearchText = searchText ?? string.Empty;
    }

    /// <summary>
    /// Mijozlarni Ismi bo'yicha filtrlaydi.
    /// null yoki bo'sh qiymat berilsa to'liq ro'yxat ko'rsatiladi.
    /// </summary>
    public void ApplyCustomerFilter(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            FilteredCustomers = AvailableCustomers;
            return;
        }

        var results = AvailableCustomers
            .Where(c => TransliterationHelper.ContainsIgnoreScript(c.Name, searchText) ||
                        TransliterationHelper.ContainsIgnoreScript(c.Phone, searchText) ||
                        TransliterationHelper.ContainsIgnoreScript(c.Address, searchText))
            .ToList();

        FilteredCustomers = new ObservableCollection<UserViewModel>(results);
    }

    private void Clear()
    {
        saleSession.ClearSession();
        SaleItems.Clear();

        Customer = null;
        CustomerInput = string.Empty;
        ProductSearchText = string.Empty;
        SelectedSaleItem = null;
        IsPopupOpen = false;
        PopupItem = null;
        Date = DateTime.Now;
        TotalAmount = null;
        FinalAmount = null;
        Note = string.Empty;
        TotalAmountWithUserBalance = null;
        EditingSaleId = 0;
        IsEditingItem = false;
        OriginalItemIndex = -1;
        _editingItemSnapshot = null;
        ClearCurrentSaleItem();
        RecalculateTotals();
    }

    private void ClearCurrentSaleItem()
    {
        CurrentSaleItem.PropertyChanged -= SaleItemPropertyChanged;
        CurrentSaleItem = new SaleItemViewModel();
        CurrentSaleItem.PropertyChanged += SaleItemPropertyChanged;
    }

    // ─────────────────────────────────────────────
    // Private Helpers
    // ─────────────────────────────────────────────

    private void RecalculateTotals()
    {
        TotalAmount = SaleItems.Sum(x => x.Amount ?? 0);
        FinalAmount = TotalAmount;

        TotalRowsCount = SaleItems.Count;
        DistinctProductsCount = SaleItems.Select(x => x.Product?.Code).Distinct().Count();
        TotalBundlesCount = SaleItems.Sum(x => x.BundleCount ?? 0);
        TotalQuantityCount = SaleItems.Sum(x => x.TotalCount ?? 0);
    }

    private void RecalculateTotalAmountWithUserBalance()
    {
        if (Customer is not null)
            TotalAmountWithUserBalance = Customer.Balance + TotalAmount;
    }

    #endregion Private helpers

    #region Load data

    // ─────────────────────────────────────────────
    // Data Loading
    // ─────────────────────────────────────────────

    private async Task EnsureInitializedAsync()
    {
        if (_initializationTask is not null)
            await _initializationTask;
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
        {
            AvailableCustomers = mapper.Map<ObservableCollection<UserViewModel>>(response.Data!);

            if (Customer != null)
            {
                var match = AvailableCustomers.FirstOrDefault(c => c.Id == Customer.Id);
                if (match != null) Customer = match;
            }
            else if (EditingSaleId == 0 && saleSession.SelectedCustomer != null)
            {
                var match = AvailableCustomers.FirstOrDefault(c => c.Id == saleSession.SelectedCustomer.Id);
                if (match != null) Customer = match;
            }
        }
        else
            ErrorMessage = response.Message ?? "Mahsulot turlarini yuklashda xatolik.";
    }

    public async Task LoadProductsAsync()
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["ProductType"] = ["include:Product"]
            }
        };

        var response = await client.ProductResidues.Filter(request).Handle(isLoading => IsLoading = isLoading);

        if (!response.IsSuccess)
        {
            ErrorMessage = response.Message ?? "Mahsulotlarni yuklashda xatolik.";
            return;
        }

        var productResidues = mapper.Map<ObservableCollection<ProductResidueViewModel>>(response.Data!);

        var allTypes = productResidues.Select(pr =>
        {
            pr.ProductType.AvailableCount = pr.Count;

            var inCart = SaleItems.FirstOrDefault(i => i.ProductType?.Id == pr.ProductType.Id);
            if (inCart != null)
            {
                pr.ProductType.AvailableCount -= (inCart.TotalCount ?? 0);
                inCart.ProductType = pr.ProductType;
                inCart.Product = pr.ProductType.Product;
            }

            return pr.ProductType;
        })
        .Where(pt => pt is not null && pt.Product is not null)
        .ToList();

        var grouped = allTypes.GroupBy(pt => pt.Product.Id);
        var products = new ObservableCollection<ProductViewModel>();

        foreach (var group in grouped)
        {
            var product = group.First().Product;
            product.ProductTypes = new ObservableCollection<ProductTypeViewModel>(group);
            products.Add(product);
        }

        AvailableProducts = products;
    }

    public async Task LoadSaleForEditAsync(long saleId, bool notifyOnLoad = true)
    {
        await EnsureInitializedAsync();

        EditingSaleId = saleId;

        SaleItems = new ObservableCollection<SaleItemViewModel>();
        SaleItems.CollectionChanged += (s, e) => RecalculateTotals();

        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["id"] = [saleId.ToString()],
                ["saleItems"] = ["include:productType.product"]
            }
        };

        var response = await client.Sales.Filter(request).Handle(isLoading => IsLoading = isLoading);

        if (!response.IsSuccess)
        {
            ErrorMessage = response.Message ?? "Savdoni yuklashda xatolik!";
            EditingSaleId = 0;
            return;
        }

        var sale = mapper.Map<SaleViewModel>(response.Data.First());

        Date = sale.Date;
        Note = sale.Note ?? string.Empty;

        var customer = AvailableCustomers.FirstOrDefault(c => c.Id == sale.CustomerId);
        if (customer is not null) Customer = customer;

        if (sale.SaleItems is not null)
        {
            foreach (var saleItemResponse in sale.SaleItems)
            {
                var productTypeResponse = saleItemResponse.ProductType;
                var productResponse = productTypeResponse?.Product;

                ProductViewModel? productVM = null;

                if (productTypeResponse != null)
                    productVM = AvailableProducts.FirstOrDefault(p => p.Id == productTypeResponse.ProductId);

                if (productVM == null && productResponse != null)
                {
                    productVM = mapper.Map<ProductViewModel>(productResponse);
                    productVM.ProductTypes ??= [];
                    AvailableProducts.Add(productVM);
                }

                ProductTypeViewModel? productTypeVM = null;

                if (productVM != null && productTypeResponse != null)
                {
                    productTypeVM = productVM.ProductTypes?.FirstOrDefault(pt => pt.Id == productTypeResponse.Id);

                    if (productTypeVM == null)
                    {
                        productTypeVM = mapper.Map<ProductTypeViewModel>(productTypeResponse);
                        productVM.ProductTypes ??= [];
                        productVM.ProductTypes.Add(productTypeVM);
                    }
                }

                if (productVM == null || productTypeVM == null) continue;

                var newItemVM = new SaleItemViewModel
                {
                    Product = productVM,
                    ProductType = productTypeVM,
                    BundleCount = saleItemResponse.BundleCount,
                    BundleItemCount = saleItemResponse.BundleItemCount,
                    UnitPrice = saleItemResponse.UnitPrice,
                    Amount = saleItemResponse.Amount,
                    TotalCount = saleItemResponse.TotalCount
                };

                newItemVM.PropertyChanged += SaleItemPropertyChanged;
                SaleItems.Add(newItemVM);
            }
        }

        RecalculateTotals();
    }

    // Qaytarishlar ro'yxatidan tanlangan qaytarishni yuklaydi (chek/dalolatnoma generatsiya qilish uchun).
    public async Task LoadReturnForEditAsync(long returnId)
    {
        await EnsureInitializedAsync();

        EditingSaleId = returnId;

        SaleItems = new ObservableCollection<SaleItemViewModel>();
        SaleItems.CollectionChanged += (s, e) => RecalculateTotals();

        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["id"] = [returnId.ToString()],
                ["customer"] = ["include"],
                ["returnItems"] = ["include:productType.product"]
            }
        };

        var response = await client.Returns.Filter(request).Handle(isLoading => IsLoading = isLoading);

        if (!response.IsSuccess || response.Data is null || response.Data.Count == 0)
        {
            ErrorMessage = response.Message ?? "Qaytarishni yuklashda xatolik!";
            EditingSaleId = 0;
            return;
        }

        var ret = response.Data.First();
        Date = ret.Date;
        Note = ret.Note ?? string.Empty;

        var customer = AvailableCustomers.FirstOrDefault(c => c.Id == ret.CustomerId);
        if (customer is not null) Customer = customer;

        if (ret.ReturnItems is not null)
        {
            foreach (var ri in ret.ReturnItems)
            {
                var ptResp = ri.ProductType;
                var pResp = ptResp?.Product;

                ProductViewModel? productVM = null;
                if (ptResp != null)
                    productVM = AvailableProducts.FirstOrDefault(p => p.Id == ptResp.ProductId);
                if (productVM == null && pResp != null)
                {
                    productVM = mapper.Map<ProductViewModel>(pResp);
                    productVM.ProductTypes ??= [];
                    AvailableProducts.Add(productVM);
                }

                ProductTypeViewModel? typeVM = null;
                if (productVM != null && ptResp != null)
                {
                    typeVM = productVM.ProductTypes?.FirstOrDefault(pt => pt.Id == ptResp.Id);
                    if (typeVM == null)
                    {
                        typeVM = mapper.Map<ProductTypeViewModel>(ptResp);
                        productVM.ProductTypes ??= [];
                        productVM.ProductTypes.Add(typeVM);
                    }
                }

                if (productVM == null || typeVM == null) continue;

                var item = new SaleItemViewModel
                {
                    Product = productVM,
                    ProductType = typeVM,
                    Unit = InferReturnUnit(ri, typeVM),
                    BundleCount = ri.BundleCount,
                    BundleItemCount = ri.BundleItemCount,
                    TotalCount = ri.TotalCount,
                    RestockCount = ri.RestockCount,
                    UnitPrice = ri.UnitPrice,
                    Amount = ri.Amount
                };

                item.PropertyChanged += SaleItemPropertyChanged;
                SaleItems.Add(item);
            }
        }

        RecalculateTotals();
    }

    // Saqlangan qaytarish qatoridan birlikni (Qop/To'plam/Dona) tiklaydi.
    // RestockCount>0 → qop; aks holda birlikdagi juft soni orqali aniqlanadi.
    private static string InferReturnUnit(ReturnItemResponse ri, ProductTypeViewModel type)
    {
        if (ri.RestockCount > 0) return "Qop";
        if (ri.BundleItemCount <= 1) return "Dona";

        var pack = type.PackItemCount ?? 0;
        if (pack > 0 && ri.BundleItemCount == pack) return "To'plam";

        var bundle = type.BundleItemCount ?? 0;
        if (bundle > 0 && ri.BundleItemCount == bundle) return "Qop";

        return "To'plam";
    }

    #endregion Load data

    #region Commands

    // ─────────────────────────────────────────────
    // Commands
    // ─────────────────────────────────────────────

    [RelayCommand]
    private void ScanReturn(string? code)
    {
        if (Customer is null)
        {
            WarningMessage = "Avval mijozni tanlang (narx mijoz valyutasida kiritiladi).";
            return;
        }

        var match = BarcodeResolver.Resolve(AvailableProducts, code);
        if (match is null)
        {
            WarningMessage = $"Shtrix-kod topilmadi: {code}";
            return;
        }

        var isQop = match.Unit == BarcodeUnit.Qop;
        var pairs = isQop
            ? match.ProductType.BundleItemCount ?? 0
            : match.ProductType.PackItemCount ?? 0;

        if (pairs <= 0)
        {
            WarningMessage = "Razmer uchun qadoqdagi soni belgilanmagan.";
            return;
        }

        // Har skaner = 1 birlik (qop yoki to'plam).
        AddReturnLine(match.Product, match.ProductType, isQop ? "Qop" : "To'plam", 1, pairs, isQop ? pairs : 0, null);
    }

    [RelayCommand]
    private void Add()
    {
        if (Customer is null)
        {
            WarningMessage = "Avval mijozni tanlang (narx mijoz valyutasida kiritiladi).";
            return;
        }

        if (Date.Date > DateTime.Today)
        {
            WarningMessage = "Kelajakdagi sanani tanlab bo'lmaydi!";
            return;
        }

        if (CurrentSaleItem.Product is null ||
            CurrentSaleItem.ProductType is null ||
            CurrentSaleItem.UnitPrice is null ||
            CurrentSaleItem.BundleCount is not > 0)
        {
            WarningMessage = "Mahsulot tanlanmagan yoki miqdor noto'g'ri!";
            return;
        }

        var quantity = CurrentSaleItem.BundleCount.Value;
        var isQop = SelectedReturnUnit == "Qop";
        var pairsPerUnit = SelectedReturnUnit switch
        {
            "To'plam" => CurrentSaleItem.ProductType.PackItemCount ?? 0,
            "Dona" => 1,
            _ => CurrentSaleItem.ProductType.BundleItemCount ?? 0
        };

        if (pairsPerUnit <= 0)
        {
            WarningMessage = "Razmer uchun qadoqdagi soni belgilanmagan.";
            return;
        }

        var totalPairs = quantity * pairsPerUnit;
        AddReturnLine(CurrentSaleItem.Product, CurrentSaleItem.ProductType, SelectedReturnUnit, quantity, pairsPerUnit, isQop ? totalPairs : 0, CurrentSaleItem.UnitPrice);

        IsEditingItem = false;
        OriginalItemIndex = -1;
        _editingItemSnapshot = null;
        ClearCurrentSaleItem();
    }

    // Qaytarish qatori — birlik (Qop/To'plam/Dona) bo'yicha alohida saqlanadi.
    // Jami juft = miqdor × birlikdagi juft. TotalCount juft-asosli (asosiy manba);
    // BundleCount faqat birlikdagi miqdorni ko'rsatish uchun (SaleItemViewModel'ning qop-asosli
    // avto-hisobini bekor qilish uchun TotalCount undan keyin qayta o'rnatiladi).
    private void AddReturnLine(ProductViewModel product, ProductTypeViewModel type, string unit, int quantity, int pairsPerUnit, int restockPairs, decimal? unitPrice)
    {
        var totalPairs = quantity * pairsPerUnit;
        if (totalPairs <= 0) return;

        var existing = SaleItems.FirstOrDefault(s => s.ProductType?.Id == type.Id && s.Unit == unit);
        if (existing is not null)
        {
            var newTotal = (existing.TotalCount ?? 0) + totalPairs;
            var newRestock = (existing.RestockCount ?? 0) + restockPairs;
            existing.BundleCount = (existing.BundleCount ?? 0) + quantity; // qop-asosli avto-hisobni ishga tushiradi
            existing.BundleItemCount = pairsPerUnit;
            existing.TotalCount = newTotal;                                 // to'g'ri juft sonini tiklaymiz
            existing.RestockCount = newRestock;
        }
        else
        {
            var item = new SaleItemViewModel { Product = product, ProductType = type, Unit = unit };

            var price = unitPrice ?? ConvertSomToCustomer(type.UnitPrice);
            if (price is not null) item.UnitPrice = price;

            item.BundleCount = quantity;
            item.BundleItemCount = pairsPerUnit;
            item.TotalCount = totalPairs;
            item.RestockCount = restockPairs;
            item.PropertyChanged += SaleItemPropertyChanged;
            SaleItems.Add(item);
        }

        RecalculateTotals();
    }

    [RelayCommand]
    private void Edit()
    {
        if (SelectedSaleItem is null) return;

        if (IsEditingItem)
        {
            WarningMessage = "Avval tahrirlashni yakunlang!";
            return;
        }

        bool hasCurrentData = CurrentSaleItem.Product is not null || CurrentSaleItem.BundleCount.HasValue;

        if (hasCurrentData)
        {
            var result = MessageBox.Show(
                "Hozirgi kiritilgan ma'lumotlar o'chib ketadi. Davom etmoqchimisiz?",
                "Ogohlantirish",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No) return;
        }

        SelectedSaleItem.ProductType!.AvailableCount += (SelectedSaleItem.TotalCount ?? 0);

        CurrentSaleItem.PropertyChanged -= SaleItemPropertyChanged;

        try
        {
            _editingItemSnapshot = SelectedSaleItem;
            OriginalItemIndex = SaleItems.IndexOf(SelectedSaleItem);
            IsEditingItem = true;

            SelectedReturnUnit = SelectedSaleItem.Unit ?? "Dona";
            CurrentSaleItem.Product = SelectedSaleItem.Product;
            CurrentSaleItem.ProductType = SelectedSaleItem.ProductType;
            CurrentSaleItem.BundleCount = SelectedSaleItem.BundleCount ?? SelectedSaleItem.TotalCount;
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
    private void CancelEdit()
    {
        if (!IsEditingItem || _editingItemSnapshot is null)
        {
            IsEditingItem = false;
            ClearCurrentSaleItem();
            return;
        }

        _editingItemSnapshot.ProductType!.AvailableCount -= (_editingItemSnapshot.TotalCount ?? 0);

        if (OriginalItemIndex >= 0 && OriginalItemIndex <= SaleItems.Count)
            SaleItems.Insert(OriginalItemIndex, _editingItemSnapshot);
        else
            SaleItems.Add(_editingItemSnapshot);

        IsEditingItem = false;
        OriginalItemIndex = -1;
        _editingItemSnapshot = null;

        ClearCurrentSaleItem();
        RecalculateTotals();
    }

    [RelayCommand]
    private void DeleteItem(SaleItemViewModel item)
    {
        if (item is null) return;

        var result = MessageBox.Show(
            "Mahsulotni o'chirishni tasdiqlaysizmi?",
            "O'chirishni tasdiqlash",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.No) return;

        item.ProductType!.AvailableCount += (item.TotalCount ?? 0);
        item.PropertyChanged -= SaleItemPropertyChanged;
        SaleItems.Remove(item);
        RecalculateTotals();
    }

    [ObservableProperty] private SaleItemViewModel? popupItem;
    [ObservableProperty] private bool isPopupOpen;

    [RelayCommand]
    private void ViewProduct(SaleItemViewModel? item)
    {
        if (item is null) return;
        PopupItem = item;
        IsPopupOpen = true;
    }

    [RelayCommand]
    private void ClosePopup()
    {
        IsPopupOpen = false;
        PopupItem = null;
    }

    [RelayCommand]
    private void ClearSale()
    {
        var result = MessageBox.Show(
            "Barcha kiritilgan ma'lumotlarni tozalashni xohlaysizmi?",
            "Tozalash",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            Clear();
            SuccessMessage = "Ma'lumotlar tozalandi.";
        }
    }

    [RelayCommand]
    private async Task Submit()
    {
        if (Date.Date > DateTime.Today)
        {
            WarningMessage = "Kelajakdagi sanani tanlab bo'lmaydi!";
            return;
        }

        if (SaleItems.Count == 0)
        {
            WarningMessage = "Hech qanday mahsulot kiritilmagan!";
            return;
        }

        if (Customer is null)
        {
            WarningMessage = "Mijoz tanlanmagan!";
            return;
        }

        ReturnRequest request = new()
        {
            Date = Date == DateTime.Today ? DateTime.Now : Date,
            CustomerId = Customer?.Id ?? 0,
            TotalAmount = FinalAmount ?? 0,
            Note = Note,
            ReturnItems = [.. SaleItems.Select(item => new ReturnItemRequest
        {
            ProductTypeId = item.ProductType!.Id,
            BundleCount = item.BundleCount ?? 0,
            TotalCount = item.TotalCount ?? 0,
            RestockCount = item.RestockCount ?? 0,
            UnitPrice = (decimal)item.UnitPrice!,
            Amount = (decimal)item.Amount!
        })]
        };

        var response = await client.Returns.Create(request).Handle(isLoading => IsLoading = isLoading);

        bool isSuccess = response.IsSuccess;

        if (isSuccess)
        {
            SuccessMessage = $"Qaytarish muvaffaqiyatli saqlandi. Mahsulotlar soni: {SaleItems.Count}";

            var result = MessageBox.Show(
                "Qaytarish muvaffaqiyatli saqlandi!\n\nChop etishni xohlaysizmi?",
                "Muvaffaqiyat",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                await ShowPrintPreview();
        }
        else
            ErrorMessage = response.Message ?? "Qaytarishni ro'yxatga olishda xatolik!";

        if (isSuccess)
        {
            Clear();
            navigation.GoBack();
        }
    }

    #endregion Commands

    #region Generate Document

    public async Task ShowPrintPreview()
    {
        if (SaleItems == null || !SaleItems.Any())
        {
            MessageBox.Show("Ko'rsatish uchun ma'lumot yo'q.", "Eslatma", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var uniqueUrls = SaleItems
                .Select(i => i.Product?.DisplayImagePath)
                .Where(url => !string.IsNullOrEmpty(url))
                .Select(url => url!)
                .Where(url => !_imageCache.ContainsKey(url))
                .Distinct()
                .ToList();

            if (uniqueUrls.Any())
            {
                var tasks = uniqueUrls.Select(async url =>
                {
                    var bitmap = await DownloadBitmapAsync(url);
                    if (bitmap != null) _imageCache[url] = bitmap;
                });
                await Task.WhenAll(tasks);
            }

            var fixedDoc = CreateFixedDocumentForPrint();

            var toolbar = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(10) };

            // SAQLASH
            var saveButton = new Button
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new MaterialDesignThemes.Wpf.PackIcon { Kind = MaterialDesignThemes.Wpf.PackIconKind.ContentSave, Width = 18, Height = 18, VerticalAlignment = VerticalAlignment.Center },
                        new TextBlock { Text = "Saqlash", Margin = new Thickness(6,0,0,0), VerticalAlignment = VerticalAlignment.Center }
                    }
                },
                Margin = new Thickness(0,0,5,0),
                Padding = new Thickness(12, 6, 12, 6),
                Background = new SolidColorBrush(Color.FromRgb(100, 100, 100)), 
                Foreground = Brushes.White,
                FontSize = 13, FontWeight = FontWeights.SemiBold,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            saveButton.Click += (s, e) =>
            {
                var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "PDF (*.pdf)|*.pdf", FileName = $"Qaytarish_{Customer?.Name ?? "Naqd"}_{DateTime.Now:dd_MM_yyyy}.pdf" };
                if (dlg.ShowDialog() == true)
                {
                    SaveFixedDocumentToPdf(fixedDoc, dlg.FileName);
                    MessageBox.Show("Saqlandi!");
                }
            };
            toolbar.Children.Add(saveButton);

            // OCHISH
            var openButton = new Button
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new MaterialDesignThemes.Wpf.PackIcon { Kind = MaterialDesignThemes.Wpf.PackIconKind.FolderOpen, Width = 18, Height = 18, VerticalAlignment = VerticalAlignment.Center },
                        new TextBlock { Text = "Ochish", Margin = new Thickness(6,0,0,0), VerticalAlignment = VerticalAlignment.Center }
                    }
                },
                Margin = new Thickness(0,0,5,0),
                Padding = new Thickness(12, 6, 12, 6),
                Background = new SolidColorBrush(Color.FromRgb(100, 100, 100)), 
                Foreground = Brushes.White,
                FontSize = 13, FontWeight = FontWeights.SemiBold,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            openButton.Click += (s, e) =>
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string folder = Path.Combine(docs, "Forex", "Qaytarishlar");
                Directory.CreateDirectory(folder);
                string fileName = $"Qaytarish_{Customer?.Name ?? "Naqd"}_{DateTime.Now:dd_MM_yyyy}.pdf";
                string path = Path.Combine(folder, fileName);
                SaveFixedDocumentToPdf(fixedDoc, path);
                try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); } catch {}
            };
            toolbar.Children.Add(openButton);

            // ULASHISH
            var shareButton = new Button
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new MaterialDesignThemes.Wpf.PackIcon { Kind = MaterialDesignThemes.Wpf.PackIconKind.ShareVariant, Width = 18, Height = 18, VerticalAlignment = VerticalAlignment.Center },
                        new TextBlock { Text = "Ulashish", Margin = new Thickness(6,0,0,0), VerticalAlignment = VerticalAlignment.Center }
                    }
                },
                Padding = new Thickness(12, 6, 12, 6),
                Background = new SolidColorBrush(Color.FromRgb(0, 136, 204)),
                Foreground = Brushes.White,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(0)
            };

            shareButton.Click += (s, e) =>
            {
                try
                {
                    string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string folder = Path.Combine(docs, "Forex", "Qaytarishlar");
                    Directory.CreateDirectory(folder);

                    string customerName = (Customer?.Name ?? "Naqd").Replace(" ", "_");
                    string fileName = $"Qaytarish_{customerName}_{DateTime.Now:dd_MM_yyyy}.pdf";
                    string path = Path.Combine(folder, fileName);

                    SaveFixedDocumentToPdf(fixedDoc, path);

                    if (File.Exists(path))
                    {
                        var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                        var viewModel = App.AppHost!.Services.GetRequiredService<TelegramShareViewModel>();
                        viewModel.PdfFilePath = path;
                        viewModel.MessageCaption = $"Qaytarish: {customerName}\nSana: {DateTime.Now:dd.MM.yyyy}";

                        var shareWindow = new TelegramShareWindow
                        {
                            DataContext = viewModel,
                            Owner = window ?? Application.Current.MainWindow,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };
                        shareWindow.ShowDialog();
                    }
                }
                catch (Exception ex) { MessageBox.Show($"Ulashishda xatolik: {ex.Message}"); }
            };

            toolbar.Children.Add(shareButton);
            var viewer = new DocumentViewer { Document = fixedDoc, Margin = new Thickness(8) };
            var layout = new DockPanel();
            DockPanel.SetDock(toolbar, Dock.Top);
            layout.Children.Add(toolbar);
            layout.Children.Add(viewer);

            var previewWindow = new Window
            {
                Title = "Qaytarish dalolatnomasi - Oldindan ko'rish",
                Width = 1050,
                Height = 850,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = layout,
                Owner = Application.Current.MainWindow
            };

            previewWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Xatolik: {ex.Message}");
        }
    }

    private FixedDocument CreateFixedDocumentForPrint()
    {
        var fixedDoc = new FixedDocument();
        const double pageWidth = 793.7;
        const double pageHeight = 1122.5;
        const double margin = 25;

        // Oxirgi betda JAMI + mijoz qoldig'i qatori uchun zaxira balandlik.
        double footerSpace = Customer?.Balance is not null ? 110 : 70;

        // 9 ta ustun: T/r, Rasm, Kod/Nomi (birlashgan), Razmer, Birlik, Miqdor, Jami soni, Narxi, Jami summa
        string[] headers = { "T/r", "Rasm", "Kod / Nomi", "Razmer", "Birlik", "Miqdor", "Jami soni", "Narxi", "Jami summa" };
        double[] widths = { 32, 118, 120, 62, 58, 58, 68, 85, 142.7 };
        double tableWidth = pageWidth - margin * 2;

        decimal grandTotalAmount = SaleItems.Sum(x => x.Amount ?? 0);
        double grandTotalBundle = SaleItems.Sum(x => (double)(x.BundleCount ?? 0));
        double grandTotalCount = SaleItems.Sum(x => (double)(x.TotalCount ?? 0));

        var groupedItems = SaleItems
            .OrderBy(i => i.Product?.Code ?? string.Empty)
            .GroupBy(i => i.Product?.Code ?? string.Empty)
            .ToList();

        // Used for pagination
        int currentGroupIndex = 0;
        int pageNumber = 1;
        int globalTr = 1;

        while (currentGroupIndex < groupedItems.Count)
        {
            var page = new FixedPage { Width = pageWidth, Height = pageHeight, Background = Brushes.White };
            var container = new StackPanel { Margin = new Thickness(margin, 20, margin, 20) };

            // currentY — sahifada qancha joy ishlatilganini kuzatib boramiz
            double currentY = 40; // StackPanel top margin

            // Sarlavha faqat 1-betda
            if (pageNumber == 1)
            {
                var title = new TextBlock
                {
                    Text = "Qaytarilgan mahsulotlar ro'yxati",
                    FontSize = 19,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(DocInk),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                title.Measure(new Size(tableWidth, double.PositiveInfinity));
                container.Children.Add(title);
                currentY += title.DesiredSize.Height + 10;

                var infoGrid = new Grid { Margin = new Thickness(0, 0, 0, 8) };
                infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var cust = new TextBlock { FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
                cust.Inlines.Add(new Run("Mijoz: ") { Foreground = new SolidColorBrush(DocMuted) });
                cust.Inlines.Add(new Run((Customer?.Name ?? "Naqd").ToUpper()) { Foreground = new SolidColorBrush(DocInk), FontWeight = FontWeights.SemiBold });

                var dt = new TextBlock { FontSize = 13, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
                dt.Inlines.Add(new Run("Sana: ") { Foreground = new SolidColorBrush(DocMuted) });
                dt.Inlines.Add(new Run($"{Date:dd.MM.yyyy}") { Foreground = new SolidColorBrush(DocInk), FontWeight = FontWeights.SemiBold });
                Grid.SetColumn(dt, 1);

                infoGrid.Children.Add(cust);
                infoGrid.Children.Add(dt);
                infoGrid.Measure(new Size(tableWidth, double.PositiveInfinity));
                container.Children.Add(infoGrid);
                currentY += infoGrid.DesiredSize.Height + 8;

                // "Qaytarildi: {izoh}" — hujjat qaytarish ekanini bildiradi; tagida mahsulotlar keladi.
                var noteBlock = new TextBlock { FontSize = 13, Margin = new Thickness(0, 0, 0, 8), TextWrapping = TextWrapping.Wrap };
                noteBlock.Inlines.Add(new Run("Qaytarildi: ") { Foreground = new SolidColorBrush(DocMuted) });
                noteBlock.Inlines.Add(new Run(string.IsNullOrWhiteSpace(Note) ? "—" : Note) { Foreground = new SolidColorBrush(DocInk), FontWeight = FontWeights.SemiBold });
                noteBlock.Measure(new Size(tableWidth, double.PositiveInfinity));
                container.Children.Add(noteBlock);
                currentY += noteBlock.DesiredSize.Height + 8;

                var divider = new Border
                {
                    Height = 2,
                    Background = new SolidColorBrush(DocAccent),
                    CornerRadius = new CornerRadius(1),
                    Margin = new Thickness(0, 0, 0, 12)
                };
                container.Children.Add(divider);
                currentY += 14;
            }

            var grid = new Grid { Width = tableWidth };
            foreach (var w in widths)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(w) });

            AddRow(grid, true, 0, headers);
            // Header balandligini o'lchash
            {
                var hb = new Border
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(0.5),
                    Background = new SolidColorBrush(Color.FromRgb(235, 235, 235)),
                    Padding = new Thickness(4, 5, 4, 5),
                    Child = new TextBlock { Text = "Jami soni", FontSize = 13, FontWeight = FontWeights.Bold }
                };
                hb.Measure(new Size(widths[6], double.PositiveInfinity));
                currentY += hb.DesiredSize.Height;
            }

            var groupsOnPage = new List<IGrouping<string, SaleItemViewModel>>();
            int tempGroupIndex = currentGroupIndex;

            // 0.5 sm = ~19px (96 DPI da 1sm = 37.8px, 0.5sm = 18.9px)
            const double safetyGap = 19.0;
            
            while (tempGroupIndex < groupedItems.Count)
            {
                var group = groupedItems[tempGroupIndex];

                // 1. Bir qator balandligini o'lchash
                double oneRowHeight = CalculateOneRowHeight(group.First(), widths);

                // 2. Guruhda nechta qator bor — shuncha ko'paytirish
                // (lekin rasm tufayli minimum 142px)
                int rowCount = group.Count();
                double groupHeight = Math.Max(oneRowHeight * rowCount, 142.0);

                // 3. Oxirgi guruh bo'lsa, Jami qatori uchun ham joy kerak
                bool isLastGroup = (tempGroupIndex == groupedItems.Count - 1);
                double jamiExtra = isLastGroup ? footerSpace : 0;

                // 4. Footer + safetyGap (0.5sm) + joriy ishlatilgan joy
                double usedSpace = currentY + groupHeight + jamiExtra + safetyGap;

                // 5. Sig'adimi?
                if (usedSpace > pageHeight - margin && tempGroupIndex > currentGroupIndex)
                    break; // Bu guruh keyingi betga

                groupsOnPage.Add(group);
                currentY += groupHeight;
                tempGroupIndex++;
            }

            // Update current index for next page
            currentGroupIndex = tempGroupIndex;

            // Jadvalni render qilish
            int gridRow = 1;
            foreach (var group in groupsOnPage)
            {
                int groupStartRow = gridRow;
                bool isFirstInGroup = true;
                int groupRowCount = group.Count();

                foreach (var item in group)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    AddCellToGrid(grid, globalTr.ToString(), gridRow, 0, false, TextAlignment.Center);
                    
                    if (isFirstInGroup)
                    {
                        var imgBorder = CreateImageCell(item.Product?.DisplayImagePath ?? string.Empty);
                        Grid.SetRow(imgBorder, groupStartRow);
                        Grid.SetColumn(imgBorder, 1);
                        Grid.SetRowSpan(imgBorder, groupRowCount);
                        grid.Children.Add(imgBorder);
                        
                        // Kod va Nom birlashgan cell (2 qatorlik: yuqorida kod, pastida nomi)
                        var codeNameBorder = CreateCodeNameCell(item.Product?.Code ?? "", item.Product?.Name ?? "");
                        Grid.SetRow(codeNameBorder, groupStartRow);
                        Grid.SetColumn(codeNameBorder, 2);
                        Grid.SetRowSpan(codeNameBorder, groupRowCount);
                        grid.Children.Add(codeNameBorder);
                        
                        isFirstInGroup = false;
                    }
                    
                    // 9 ustun: Razmer=3, Birlik=4, Miqdor=5, Jami soni=6, Narxi=7, Jami summa=8
                    AddCellToGrid(grid, item.ProductType?.Type ?? "", gridRow, 3, false, TextAlignment.Center);
                    AddCellToGrid(grid, item.Unit ?? "", gridRow, 4, false, TextAlignment.Center);
                    AddCellToGrid(grid, item.BundleCount?.ToString("N0") ?? "0", gridRow, 5, false, TextAlignment.Right);
                    AddCellToGrid(grid, item.TotalCount?.ToString("N0") ?? "0", gridRow, 6, false, TextAlignment.Right);
                    AddCellToGrid(grid, item.UnitPrice?.ToString("N2") ?? "0.00", gridRow, 7, false, TextAlignment.Right);
                    AddCellToGrid(grid, item.Amount?.ToString("N2") ?? "0.00", gridRow, 8, false, TextAlignment.Right);

                    gridRow++;
                    globalTr++;
                }
            }

            // Jami qatori faqat oxirgi betda (barcha guruhlar tugagan bo'lsa)
            if (currentGroupIndex >= groupedItems.Count)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });

                var totalBg = new SolidColorBrush(DocHeaderBg);

                // JAMI: 0-4 ustunlar (5 ta)
                var totalLabel = CreateCell("JAMI:", true, TextAlignment.Right);
                totalLabel.Background = totalBg;
                Grid.SetRow(totalLabel, gridRow);
                Grid.SetColumn(totalLabel, 0);
                Grid.SetColumnSpan(totalLabel, 5);
                grid.Children.Add(totalLabel);

                // Miqdor: index 5, Jami soni: index 6
                AddCellToGrid(grid, grandTotalBundle.ToString("N0"), gridRow, 5, true, TextAlignment.Right);
                AddCellToGrid(grid, grandTotalCount.ToString("N0"), gridRow, 6, true, TextAlignment.Right);

                // Narxi + Jami summa: 7-8 ustunlar (2 ta)
                var totalSumCell = CreateCell(grandTotalAmount.ToString("N2"), true, TextAlignment.Right);
                totalSumCell.Background = totalBg;
                Grid.SetRow(totalSumCell, gridRow);
                Grid.SetColumn(totalSumCell, 7);
                Grid.SetColumnSpan(totalSumCell, 2);

                if (totalSumCell.Child is TextBlock tbSum)
                {
                    tbSum.FontSize = 14;
                    tbSum.FontWeight = FontWeights.Bold;
                    tbSum.Foreground = new SolidColorBrush(DocAccent);
                }

                grid.Children.Add(totalSumCell);
                gridRow++;
                
                // Mijoz qoldig'i (qarzdorlik/haqdorlik)
                if (Customer?.Balance is not null)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
                    
                    var balanceLabel = CreateCell("Joriy qoldiq:", true, TextAlignment.Right);
                    balanceLabel.Background = new SolidColorBrush(Color.FromRgb(230, 240, 255));
                    Grid.SetRow(balanceLabel, gridRow);
                    Grid.SetColumn(balanceLabel, 0);
                    Grid.SetColumnSpan(balanceLabel, 7);
                    grid.Children.Add(balanceLabel);
                    
                    var balanceValue = Customer.Balance.Value + grandTotalAmount;
                    var balanceText = balanceValue >= 0
                        ? $"+{balanceValue:N0} (Haqdor)" 
                        : $"{balanceValue:N0} (Qarzdor)";
                    
                    var balanceCell = CreateCell(balanceText, true, TextAlignment.Right);
                    balanceCell.Background = new SolidColorBrush(Color.FromRgb(230, 240, 255));
                    Grid.SetRow(balanceCell, gridRow);
                    Grid.SetColumn(balanceCell, 7);
                    Grid.SetColumnSpan(balanceCell, 2);
                    
                    if (balanceCell.Child is TextBlock tbBal)
                    {
                        tbBal.FontSize = 13;
                        tbBal.Foreground = balanceValue >= 0 
                            ? new SolidColorBrush(Color.FromRgb(0, 128, 0)) 
                            : new SolidColorBrush(Color.FromRgb(200, 0, 0));
                    }
                    
                    grid.Children.Add(balanceCell);
                }
            }

            container.Children.Add(grid);
            page.Children.Add(container);

            var pnb = new TextBlock
            {
                Text = $"{pageNumber}-bet",
                FontSize = 10,
                Foreground = Brushes.Gray
            };
            FixedPage.SetRight(pnb, margin);
            FixedPage.SetBottom(pnb, 15);
            page.Children.Add(pnb);

            var pc = new PageContent();
            ((IAddChild)pc).AddChild(page);
            fixedDoc.Pages.Add(pc);

            currentGroupIndex = tempGroupIndex;
            pageNumber++;
        }

        return fixedDoc;
    }

    // Bir qatorning (bitta item) balandligini o'lchaydi
    private double CalculateOneRowHeight(SaleItemViewModel item, double[] widths)
    {
        double rowH = 0;
        // 8 ta ustun: T/r(0), Rasm(1), Kod/Nomi(2), Razmer(3), Qop soni(4), Jami soni(5), Narxi(6), Jami summa(7)
        var cells = new[]
        {
            ($"{item.Product?.Code ?? string.Empty}\n{item.Product?.Name ?? string.Empty}", widths[2]),
            (item.ProductType?.Type ?? "",                 widths[3]),
            (item.Unit ?? "",                              widths[4]),
            (item.BundleCount?.ToString("N0") ?? "0",      widths[5]),
            (item.TotalCount?.ToString("N0")  ?? "0",      widths[6]),
            (item.UnitPrice?.ToString("N2")   ?? "0.00",   widths[7]),
            (item.Amount?.ToString("N2")      ?? "0.00",   widths[8]),
        };

        foreach (var (text, width) in cells)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0.5),
                Padding = new Thickness(4, 5, 4, 5),
                Child = new TextBlock { Text = text, FontSize = 12, TextWrapping = TextWrapping.Wrap }
            };
            border.Measure(new Size(width, double.PositiveInfinity));
            if (border.DesiredSize.Height > rowH)
                rowH = border.DesiredSize.Height;
        }

        return rowH;
    }
    private async Task<BitmapSource?> DownloadBitmapAsync(string url)
    {
        try
        {
            byte[] data = await _httpClient.GetByteArrayAsync(url);
            using MemoryStream ms = new(data);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = ms;
            bitmap.DecodePixelWidth = 300;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch { return null; }
    }

    private Border CreateImageCell(string imagePath)
    {
        var border = new Border
        {
            Width = 140,
            Height = 140,
            BorderBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
            BorderThickness = new Thickness(1),
            Background = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            ClipToBounds = true
        };

        if (!string.IsNullOrEmpty(imagePath) && _imageCache.TryGetValue(imagePath, out var bitmap))
        {
            var img = new Image 
            { 
                Source = bitmap, 
                Stretch = Stretch.UniformToFill,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
            border.Child = img;
        }
        else
        {
            border.Child = new TextBlock
            {
                Text = "Rasm yo'q",
                FontSize = 8,
                Foreground = Brushes.LightGray,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }

        return border;
    }

    private void AddCellToGrid(Grid grid, string text, int row, int col, bool isHeader, TextAlignment align)
    {
        var cell = CreateCell(text, isHeader, align);
        Grid.SetRow(cell, row);
        Grid.SetColumn(cell, col);
        grid.Children.Add(cell);
    }

    private void AddRow(Grid grid, bool isHeader, int row, params string[] values)
    {
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // 9 ta ustun: T/r, Rasm, Kod/Nomi, Razmer, Birlik, Miqdor, Jami soni, Narxi, Jami summa
        TextAlignment[] alignments =
        {
            TextAlignment.Center, TextAlignment.Center, TextAlignment.Left,
            TextAlignment.Center, TextAlignment.Center, TextAlignment.Right,
            TextAlignment.Right, TextAlignment.Right, TextAlignment.Right
        };

        for (int i = 0; i < values.Length; i++)
        {
            TextAlignment finalAlignment = isHeader ? TextAlignment.Center : alignments[i];
            var cell = CreateCell(values[i], isHeader, finalAlignment);
            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, i);
            grid.Children.Add(cell);
        }
    }

    private Border CreateCell(string text, bool isHeader, TextAlignment alignment = TextAlignment.Left)
    {
        var border = new Border
        {
            BorderBrush = new SolidColorBrush(DocLine),
            BorderThickness = new Thickness(0.5),
            Background = isHeader ? new SolidColorBrush(DocHeaderBg) : Brushes.White,
            Padding = new Thickness(4, 5, 4, 5)
        };

        var tb = new TextBlock
        {
            Text = text,
            FontSize = isHeader ? 12.5 : 12,
            FontWeight = isHeader ? FontWeights.SemiBold : FontWeights.Normal,
            Foreground = isHeader ? new SolidColorBrush(DocHeaderFg) : new SolidColorBrush(DocInk),
            TextAlignment = alignment,
            VerticalAlignment = VerticalAlignment.Center
        };

        border.Child = tb;
        return border;
    }

    private Border CreateCodeNameCell(string code, string name)
    {
        var border = new Border
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(0.5),
            Background = Brushes.White,
            Padding = new Thickness(6, 4, 6, 4)
        };

        var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

        var codeBlock = new TextBlock
        {
            Text = code,
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0, 100, 180))
        };

        var nameBlock = new TextBlock
        {
            Text = name,
            FontSize = 11,
            Foreground = Brushes.DimGray,
            TextWrapping = TextWrapping.Wrap
        };

        stack.Children.Add(codeBlock);
        stack.Children.Add(nameBlock);
        border.Child = stack;

        return border;
    }

    private void SaveFixedDocumentToPdf(FixedDocument doc, string path, int dpi = 288)
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
                int pixelWidth = (int)(fixedPage.Width * scale);
                int pixelHeight = (int)(fixedPage.Height * scale);

                var bitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32);
                bitmap.Render(fixedPage);

                var encoder = new JpegBitmapEncoder { QualityLevel = 95 };
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using var ms = new MemoryStream();
                encoder.Save(ms);
                ms.Position = 0;

                var pdfPage = pdfDoc.AddPage();
                pdfPage.Width = PdfSharp.Drawing.XUnit.FromMillimeter(210);
                pdfPage.Height = PdfSharp.Drawing.XUnit.FromMillimeter(297);

                using var xgfx = PdfSharp.Drawing.XGraphics.FromPdfPage(pdfPage);
                using var ximg = PdfSharp.Drawing.XImage.FromStream(ms);

                xgfx.DrawImage(ximg, new PdfSharp.Drawing.XRect(0, 0, pdfPage.Width.Point, pdfPage.Height.Point));
            }

            pdfDoc.Save(path);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"PDF saqlashda xatolik: {ex.Message}", "Xatolik", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion Generate Document
}
