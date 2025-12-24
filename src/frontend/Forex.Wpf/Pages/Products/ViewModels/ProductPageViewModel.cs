namespace Forex.Wpf.Pages.Products.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Enums;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Common.Interfaces;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.Pages.Products.Views;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System.Collections.ObjectModel;
using System.Windows;

public partial class ProductPageViewModel : ViewModelBase, INavigationAware
{
    private readonly ForexClient client;
    private readonly IMapper mapper;
    private readonly INavigationService navigation;

    public ProductPageViewModel(IMapper mapper, ForexClient client, INavigationService navigation)
    {
        this.mapper = mapper;
        this.client = client;
        this.navigation = navigation;
        CurrentProductEntry = new ProductEntryViewModel();
        CurrentProductEntry.PropertyChanged += OnCurrentProductEntryPropertyChanged;
        _ = LoadDataAsync();
    }

    [ObservableProperty] private ObservableCollection<ProductEntryViewModel> productEntries = [];
    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];
    public string[] ProductionOrigins { get; set; } = Enum.GetNames<ProductionOrigin>();
    [ObservableProperty] private DateTime beginDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    [ObservableProperty] private DateTime endDate = DateTime.Today;
    [ObservableProperty] private ProductEntryViewModel currentProductEntry = new();
    [ObservableProperty] private ProductEntryViewModel? selectedProductEntry = default;

    #region Load Data

    private async Task LoadDataAsync()
    {
        await LoadProductsAsync();
        await LoadProductEntriesAsync();
    }

    private async Task LoadProductsAsync()
    {
        var response = await client.Products.GetAllAsync()
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
            AvailableProducts = mapper.Map<ObservableCollection<ProductViewModel>>(response.Data);
        else
            ErrorMessage = response.Message ?? "Mahsulotlarni yuklashda xatolik!";
    }

    private async Task LoadProductEntriesAsync()
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["producttype"] = ["include:product"],
                ["date"] = [$">={BeginDate:o}", $"<{EndDate.AddDays(1):o}"]
            },
            Descending = true,
            SortBy = "date"
        };

        var response = await client.ProductEntries.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            ProductEntries = mapper.Map<ObservableCollection<ProductEntryViewModel>>(response.Data);
        }
        else
            ErrorMessage = response.Message ?? "Product kirimi ma'lumotlarini yuklashda xatolik!";
    }

    private async Task LoadProductTypesForProduct(ProductViewModel product)
    {
        if (product.ProductTypes?.Any() == true || product.Id <= 0)
            return;

        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["productid"] = [product.Id.ToString()]
            }
        };

        var response = await client.ProductTypes.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            product.ProductTypes = mapper.Map<ObservableCollection<ProductTypeViewModel>>(response.Data);
        }
        else
            ErrorMessage = response.Message ?? "Maxsulot turlarini yuklashda xatolik";
    }

    #endregion

    #region Property Change Handlers

    private async void OnCurrentProductEntryPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProductEntryViewModel.Product))
        {
            if (CurrentProductEntry.Product is not null && CurrentProductEntry.Product.Id > 0)
            {
                await LoadProductTypesForProduct(CurrentProductEntry.Product);
            }
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void RedirectToAddPage()
    {
        navigation.NavigateTo(new ProductEntryPage());
    }

    [RelayCommand]
    private async Task FilterProductEntries()
    {
        await LoadProductEntriesAsync();
    }

    [RelayCommand]
    private async Task Delete(ProductEntryViewModel value)
    {
        if (value is null)
            return;

        var result = MessageBox.Show(
            $"Mahsulotni o'chirishni tasdiqlaysizmi?",
            "O'chirishni tasdiqlash",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.No)
            return;

        var response = await client.ProductEntries.Delete(value.Id)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            ProductEntries.Remove(value);
            SuccessMessage = "Mahsulot kirimi muvaffaqiyatli o'chirildi";
        }
        else
            ErrorMessage = response.Message ?? "Mahsulot kirimini o'chirishda xatolik";
    }


    private ProductEntryViewModel? _backupProductEntry = null;
    [RelayCommand]
    public async Task Edit()
    {
        if (SelectedProductEntry is null)
            return;

        bool hasCurrentData = CurrentProductEntry.Product is not null ||
                             CurrentProductEntry.BundleCount.HasValue ||
                             CurrentProductEntry.Count.HasValue;

        if (hasCurrentData && IsEditing)
        {
            var result = MessageBox.Show(
                "Hozirgi kiritilgan ma'lumotlar o'chib ketadi. Davom etmoqchimisiz?",
                "Ogohlantirish",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
                return;
        }

        // BACKUP YARATISH - Edit boshida
        _backupProductEntry = new ProductEntryViewModel
        {
            Id = SelectedProductEntry.Id,
            Date = SelectedProductEntry.Date,
            BundleCount = SelectedProductEntry.BundleCount,
            Count = SelectedProductEntry.Count,
            ProductionOrigin = SelectedProductEntry.ProductionOrigin,
            ProductionOriginName = SelectedProductEntry.ProductionOrigin.ToString(),
            ProductType = SelectedProductEntry.ProductType
        };

        // ... qolgan kod o'zgarishsiz davom etadi
        if (!AvailableProducts.Any())
        {
            await LoadProductsAsync();
        }

        CurrentProductEntry.PropertyChanged -= OnCurrentProductEntryPropertyChanged;

        try
        {
            var productCode = SelectedProductEntry.ProductType?.Product?.Code;
            var productName = SelectedProductEntry.ProductType?.Product?.Name;
            var productId = SelectedProductEntry.ProductType?.Product?.Id ?? 0;

            if (string.IsNullOrEmpty(productCode))
            {
                WarningMessage = "Mahsulot ma'lumotlari topilmadi!";
                return;
            }

            var matchingProduct = AvailableProducts.FirstOrDefault(p => p.Code == productCode);

            if (matchingProduct is not null)
            {
                await LoadProductTypesForProduct(matchingProduct);
                CurrentProductEntry.Product = matchingProduct;

                var matchingType = matchingProduct.ProductTypes?.FirstOrDefault(t =>
                    t.Type == SelectedProductEntry.ProductType?.Type);

                if (matchingType is not null)
                {
                    matchingType.BundleItemCount = SelectedProductEntry.ProductType?.BundleItemCount ?? matchingType.BundleItemCount;
                    matchingType.UnitPrice = SelectedProductEntry.ProductType?.UnitPrice ?? matchingType.UnitPrice;
                    CurrentProductEntry.Product.SelectedType = matchingType;
                }
                else if (SelectedProductEntry.ProductType is not null)
                {
                    var newType = new ProductTypeViewModel
                    {
                        Id = SelectedProductEntry.ProductType.Id,
                        Type = SelectedProductEntry.ProductType.Type ?? "",
                        BundleItemCount = SelectedProductEntry.ProductType.BundleItemCount,
                        UnitPrice = SelectedProductEntry.ProductType.UnitPrice,
                        ProductTypeItems = []
                    };

                    matchingProduct.ProductTypes ??= new ObservableCollection<ProductTypeViewModel>();
                    matchingProduct.ProductTypes.Add(newType);
                    CurrentProductEntry.Product.SelectedType = newType;
                }
            }
            else
            {
                var newProduct = new ProductViewModel
                {
                    Id = productId,
                    Code = productCode,
                    Name = productName ?? "",
                    ProductionOrigin = SelectedProductEntry.ProductionOrigin,
                    ProductTypes = []
                };

                if (productId > 0)
                {
                    await LoadProductTypesForProduct(newProduct);
                }

                if (SelectedProductEntry.ProductType is not null)
                {
                    var matchingType = newProduct.ProductTypes?.FirstOrDefault(t =>
                        t.Type == SelectedProductEntry.ProductType.Type);

                    if (matchingType is not null)
                    {
                        matchingType.BundleItemCount = SelectedProductEntry.ProductType.BundleItemCount ?? matchingType.BundleItemCount;
                        matchingType.UnitPrice = SelectedProductEntry.ProductType.UnitPrice ?? matchingType.UnitPrice;
                        newProduct.SelectedType = matchingType;
                    }
                    else
                    {
                        var newType = new ProductTypeViewModel
                        {
                            Id = SelectedProductEntry.ProductType.Id,
                            Type = SelectedProductEntry.ProductType.Type ?? "",
                            BundleItemCount = SelectedProductEntry.ProductType.BundleItemCount,
                            UnitPrice = SelectedProductEntry.ProductType.UnitPrice,
                            ProductTypeItems = []
                        };

                        newProduct.ProductTypes!.Add(newType);
                        newProduct.SelectedType = newType;
                    }
                }

                CurrentProductEntry.Product = newProduct;

                if (!AvailableProducts.Any(p => p.Code == productCode))
                {
                    AvailableProducts.Add(newProduct);
                }
            }

            CurrentProductEntry.Id = SelectedProductEntry.Id;
            CurrentProductEntry.Date = SelectedProductEntry.Date;
            CurrentProductEntry.BundleCount = SelectedProductEntry.BundleCount;
            CurrentProductEntry.Count = SelectedProductEntry.Count;
            CurrentProductEntry.ProductionOrigin = SelectedProductEntry.ProductionOrigin;
            CurrentProductEntry.ProductionOriginName = SelectedProductEntry.ProductionOrigin.ToString();

            IsEditing = true;

            ProductEntries.Remove(SelectedProductEntry);
            SelectedProductEntry = null;
        }
        finally
        {
            CurrentProductEntry.PropertyChanged += OnCurrentProductEntryPropertyChanged;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        if (_backupProductEntry is null)
        {
            WarningMessage = "Bekor qilinadigan o'zgarish yo'q!";
            return;
        }

        // Backup'dan ma'lumotlarni qaytarish
        ProductEntries.Add(_backupProductEntry);

        // Edit rejimini o'chirish va formani tozalash
        IsEditing = false;
        ClearCurrentEntry();

        // Backup'ni tozalash
        _backupProductEntry = null;
    }

    [RelayCommand]
    private async Task Update()
    {
        if (CurrentProductEntry is null || CurrentProductEntry.Id <= 0)
        {
            ErrorMessage = "Yangilanishi kerak bo'lgan ma'lumot tanlanmagan!";
            return;
        }

        if (CurrentProductEntry.Product is null)
        {
            WarningMessage = "Mahsulot tanlanmagan!";
            return;
        }

        if (CurrentProductEntry.Product.SelectedType is null)
        {
            WarningMessage = "Razmer tanlanmagan!";
            return;
        }

        if (CurrentProductEntry.BundleCount <= 0)
        {
            WarningMessage = "Qop sonini kiriting!";
            return;
        }

        if (CurrentProductEntry.Product.SelectedType.BundleItemCount <= 0)
        {
            WarningMessage = "Qopdagi sonni kiriting!";
            return;
        }

        if (CurrentProductEntry.Product.SelectedType.UnitPrice <= 0)
        {
            WarningMessage = "Tannarxni kiriting!";
            return;
        }

        var request = new ProductEntryRequest
        {
            Id = CurrentProductEntry.Id,
            Date = CurrentProductEntry.Date,
            Count = CurrentProductEntry.Count,
            BundleItemCount = CurrentProductEntry.Product.SelectedType.BundleItemCount!.Value,
            PreparationCostPerUnit = 0,
            UnitPrice = CurrentProductEntry.Product.SelectedType.UnitPrice!.Value,
            ProductionOrigin = CurrentProductEntry.ProductionOrigin,
            Product = new ProductRequest
            {
                Id = CurrentProductEntry.Product.Id,
                Code = CurrentProductEntry.Product.Code,
                Name = CurrentProductEntry.Product.Name,
                ProductionOrigin = CurrentProductEntry.ProductionOrigin,
                ProductTypes =
                [
                    new() {
                    Id = CurrentProductEntry.Product.SelectedType.Id,
                    Type = CurrentProductEntry.Product.SelectedType.Type,
                    BundleItemCount = (int)CurrentProductEntry.Product.SelectedType.BundleItemCount.Value,
                    UnitPrice = CurrentProductEntry.Product.SelectedType.UnitPrice.Value,
                    ProductTypeItems = []
                }
                ]
            }
        };

        var response = await client.ProductEntries.Update(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            SuccessMessage = "Mahsulot kirimi muvaffaqiyatli yangilandi!";

            IsEditing = false;
            ClearCurrentEntry();

            // BACKUP'NI TOZALASH
            _backupProductEntry = null;

            await LoadProductEntriesAsync();
        }
        else
        {
            ErrorMessage = response.Message ?? "Mahsulot kirimini yangilashda xatolik";
        }
    }

    #endregion

    #region Private Helpers

    private void ClearCurrentEntry()
    {
        CurrentProductEntry.PropertyChanged -= OnCurrentProductEntryPropertyChanged;

        CurrentProductEntry.Id = 0;
        CurrentProductEntry.Product = null;
        CurrentProductEntry.BundleCount = null;
        CurrentProductEntry.Count = null;
        CurrentProductEntry.ProductionOriginName = null!;

        CurrentProductEntry.PropertyChanged += OnCurrentProductEntryPropertyChanged;
    }

    public void OnNavigatedTo()
    {
        _ = LoadDataAsync();
    }

    public void OnNavigatedFrom()
    {
    }

    #endregion
}