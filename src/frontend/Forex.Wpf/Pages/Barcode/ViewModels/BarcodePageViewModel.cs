namespace Forex.Wpf.Pages.Barcode.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

public partial class BarcodePageViewModel : ViewModelBase
{
    private readonly ForexClient client;
    private readonly IMapper mapper;
    private List<ProductViewModel> matched = [];

    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];
    [ObservableProperty] private ObservableCollection<ProductViewModel> displayProducts = [];
    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private ObservableCollection<string> originOptions = ["Hammasi", "Tayyor", "Aralash", "Eva"];
    [ObservableProperty] private string selectedOrigin = "Hammasi";
    [ObservableProperty] private int resultCount;
    [ObservableProperty] private int currentPage = 1;
    [ObservableProperty] private int pageCount = 1;
    [ObservableProperty] private ObservableCollection<int> pageSizeOptions = [15, 30, 50, 80];
    [ObservableProperty] private int selectedPageSize = 30;

    public string PageInfo => $"{CurrentPage} / {PageCount}";
    public bool CanPrev => CurrentPage > 1;
    public bool CanNext => CurrentPage < PageCount;

    [ObservableProperty] private ProductViewModel? selectedProduct;
    [ObservableProperty] private ProductViewModel? detailProduct;
    [ObservableProperty] private ProductTypeViewModel? selectedType;
    [ObservableProperty] private ObservableCollection<string> units = ["Qop", "To'plam"];
    [ObservableProperty] private string selectedUnit = "Qop";

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string size = string.Empty;
    [ObservableProperty] private string unitLabel = string.Empty;
    [ObservableProperty] private int pairs;
    [ObservableProperty] private string? barcode;
    [ObservableProperty] private string? imagePath;
    [ObservableProperty] private ImageSource? barcodeImage;
    [ObservableProperty] private int copies = 1;
    [ObservableProperty] private bool isPopupOpen;

    public BarcodePageViewModel(ForexClient client, IMapper mapper)
    {
        this.client = client;
        this.mapper = mapper;
        _ = LoadProductsAsync();
    }

    public async Task LoadProductsAsync()
    {
        var response = await client.Products.GetAllAsync().Handle(loading => IsLoading = loading);
        if (!response.IsSuccess || response.Data is null)
        {
            ErrorMessage = response.Message ?? "Mahsulotlarni yuklashda xatolik!";
            return;
        }
        AvailableProducts = mapper.Map<ObservableCollection<ProductViewModel>>(response.Data);
    }

    partial void OnAvailableProductsChanged(ObservableCollection<ProductViewModel> value) => ApplyFilter();

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedOriginChanged(string value) => ApplyFilter();

    partial void OnSelectedPageSizeChanged(int value) => ApplyFilter();

    partial void OnCurrentPageChanged(int value)
    {
        OnPropertyChanged(nameof(PageInfo));
        OnPropertyChanged(nameof(CanPrev));
        OnPropertyChanged(nameof(CanNext));
    }

    partial void OnPageCountChanged(int value)
    {
        OnPropertyChanged(nameof(PageInfo));
        OnPropertyChanged(nameof(CanPrev));
        OnPropertyChanged(nameof(CanNext));
    }

    private void ApplyFilter()
    {
        matched = AvailableProducts?.Where(Matches).ToList() ?? [];
        ResultCount = matched.Count;
        PageCount = Math.Max(1, (matched.Count + SelectedPageSize - 1) / SelectedPageSize);
        CurrentPage = 1;
        RenderPage();
    }

    private void RenderPage()
    {
        var slice = matched.Skip((CurrentPage - 1) * SelectedPageSize).Take(SelectedPageSize).ToList();

        for (var i = DisplayProducts.Count - 1; i >= 0; i--)
            if (!slice.Contains(DisplayProducts[i]))
                DisplayProducts.RemoveAt(i);

        for (var i = 0; i < slice.Count; i++)
        {
            if (i >= DisplayProducts.Count)
                DisplayProducts.Add(slice[i]);
            else if (!ReferenceEquals(DisplayProducts[i], slice[i]))
            {
                DisplayProducts.Remove(slice[i]);
                DisplayProducts.Insert(i, slice[i]);
            }
        }
    }

    private bool Matches(ProductViewModel p)
    {
        if (SelectedOrigin != "Hammasi" &&
            !string.Equals(p.ProductionOrigin.ToString(), SelectedOrigin, StringComparison.OrdinalIgnoreCase))
            return false;

        if (string.IsNullOrWhiteSpace(SearchText)) return true;

        var s = SearchText.Trim();
        return TransliterationHelper.ContainsIgnoreScript(p.Name ?? string.Empty, s) ||
               TransliterationHelper.ContainsIgnoreScript(p.Code ?? string.Empty, s);
    }

    [RelayCommand]
    private void FirstPage()
    {
        if (CurrentPage <= 1) return;
        CurrentPage = 1;
        RenderPage();
    }

    [RelayCommand]
    private void PrevPage()
    {
        if (CurrentPage <= 1) return;
        CurrentPage--;
        RenderPage();
    }

    [RelayCommand]
    private void NextPage()
    {
        if (CurrentPage >= PageCount) return;
        CurrentPage++;
        RenderPage();
    }

    [RelayCommand]
    private void LastPage()
    {
        if (CurrentPage >= PageCount) return;
        CurrentPage = PageCount;
        RenderPage();
    }

    partial void OnSelectedProductChanged(ProductViewModel? value)
    {
        if (value is not null)
            DetailProduct = value;
    }

    partial void OnDetailProductChanged(ProductViewModel? value)
    {
        SelectedUnit = "Qop";
        var firstType = value?.ProductTypes?.FirstOrDefault();
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.Background,
            new Action(() => SelectedType = firstType));
    }

    [RelayCommand]
    private void Scan(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return;

        var match = BarcodeResolver.Resolve(AvailableProducts, code.Trim());
        if (match is null)
        {
            WarningMessage = "Bu barkod bo'yicha mahsulot topilmadi.";
            return;
        }

        Copies = 1;
        OpenPopup(match.Product, match.ProductType, match.Unit == BarcodeUnit.Pack);
    }

    [RelayCommand]
    private void Show()
    {
        var product = DetailProduct;
        var type = SelectedType ?? product?.ProductTypes?.FirstOrDefault();
        if (product is null || type is null)
        {
            WarningMessage = "Mahsulot va razmerni tanlang.";
            return;
        }

        if (Copies < 1) Copies = 1;
        OpenPopup(product, type, SelectedUnit == "To'plam");
    }

    private void OpenPopup(ProductViewModel product, ProductTypeViewModel type, bool isPack)
    {
        var code = isPack ? type.PackBarcode : type.QopBarcode;
        if (string.IsNullOrWhiteSpace(code))
        {
            WarningMessage = "Bu razmer uchun barkod yo'q. Sozlamalar → Mahsulotlar'da barkod yarating.";
            return;
        }

        Title = $"{product.Code} {product.Name}".Trim();
        Size = type.Type;
        UnitLabel = isPack ? "TO'PLAM" : "QOP";
        Pairs = isPack ? (type.PackItemCount ?? 0) : (type.BundleItemCount ?? 0);
        Barcode = code;
        BarcodeImage = BarcodeImageService.Render(code);
        ImagePath = product.DisplayImagePath;
        IsPopupOpen = true;
    }

    [RelayCommand]
    private void Print()
    {
        if (!IsPopupOpen || string.IsNullOrWhiteSpace(Barcode)) return;

        var count = Math.Clamp(Copies, 1, 500);
        Copies = count;

        var label = new LabelItem(Title, Size, UnitLabel, Pairs, Barcode);
        if (LabelPrintService.Print([label], AppPreferences.Instance.LabelPrinter, count))
        {
            SuccessMessage = $"{count} ta yorliq chop etishga yuborildi.";
            ClosePopup();
        }
    }

    [RelayCommand]
    private void IncrementCopies()
    {
        if (Copies < 500) Copies++;
    }

    [RelayCommand]
    private void DecrementCopies()
    {
        if (Copies > 1) Copies--;
    }

    [RelayCommand]
    private void ClosePopup() => IsPopupOpen = false;
}
