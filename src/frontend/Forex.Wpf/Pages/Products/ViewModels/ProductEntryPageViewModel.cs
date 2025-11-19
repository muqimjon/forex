namespace Forex.Wpf.Pages.Products.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

public partial class ProductEntryPageViewModel : ViewModelBase
{
    private readonly ForexClient Client = App.AppHost!.Services.GetRequiredService<ForexClient>();
    private readonly IMapper Mapper = App.AppHost!.Services.GetRequiredService<IMapper>();

    public ProductEntryPageViewModel()
    {
        CurrentProductEntry = new ProductEntryViewModel();
        CurrentProductEntry.PropertyChanged += OnCurrentProductEntryPropertyChanged;
        _ = LoadDataAsync();
    }

    [ObservableProperty] private DateTime date = DateTime.Now;
    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];
    [ObservableProperty] private ObservableCollection<ProductEntryViewModel> productEntries = [];
    [ObservableProperty] private ProductEntryViewModel currentProductEntry = new();

    #region Load Data
    public async Task LoadDataAsync()
    {
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        var response = await Client.Products.GetAllAsync()
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
            AvailableProducts = Mapper.Map<ObservableCollection<ProductViewModel>>(response.Data);
        else
            ErrorMessage = response.Message ?? "Mahsulotlarni yuklashda xatolik!";
    }

    private async Task LoadProductTypesAsync(long productId)
    {
        FilteringRequest request = new();

        var response = await Client.ProductTypes.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess && response.Data is not null)
        {
            CurrentProductEntry.AvailableProductTypes =
                Mapper.Map<ObservableCollection<ProductTypeViewModel>>(response.Data);
        }
        else
        {
            CurrentProductEntry.AvailableProductTypes = [];
        }
    }
    #endregion

    #region Property Change Handlers
    private async void OnCurrentProductEntryPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProductEntryViewModel.Product))
        {
            // Faqat combobox selection o'zgarganda ishga tushadi
            if (CurrentProductEntry.Product is not null &&
                AvailableProducts.Any(p => p.Id == CurrentProductEntry.Product.Id))
            {
                await LoadProductTypesAsync(CurrentProductEntry.Product.Id);
            }
        }
    }

    /// <summary>
    /// LostFocus event'dan chaqiriladigan metod - qo'lda kiritilgan kodni tekshiradi
    /// </summary>
    public async Task ValidateProductCodeAsync(string productCode)
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            CurrentProductEntry.AvailableProductTypes = [];
            return;
        }

        productCode = productCode.Trim();

        // Mahsulot mavjudligini tekshirish
        var existingProduct = AvailableProducts.FirstOrDefault(p =>
            p.Code?.Trim().Equals(productCode, StringComparison.OrdinalIgnoreCase) == true);

        if (existingProduct is not null)
        {
            // Mavjud mahsulot - to'liq ma'lumotlarni o'rnatish
            CurrentProductEntry.Product = existingProduct;

            // Typelarni yuklash
            await LoadProductTypesAsync(existingProduct.Id);
        }
        else
        {
            // Yangi mahsulot - tasdiqlash so'rash
            var result = MessageBox.Show(
                $"'{productCode}' kodli mahsulot topilmadi. Yangi mahsulot qo'shmoqchimisiz?",
                "Tasdiqlash",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                // Foydalanuvchi rad etdi - ma'lumotlarni tozalash
                ClearCurrentEntry();
                return;
            }

            // Foydalanuvchi tasdiqladi - yangi mahsulot yaratish
            var newProduct = new ProductViewModel
            {
                Code = productCode,
                Name = string.Empty // Foydalanuvchi nomni keyinroq kiritadi
            };

            CurrentProductEntry.Product = newProduct;

            // Yangi mahsulot uchun bo'sh type kolleksiyasi
            CurrentProductEntry.AvailableProductTypes = [];
        }
    }

    /// <summary>
    /// LostFocus event'dan chaqiriladigan metod - qo'lda kiritilgan type ni tekshiradi
    /// </summary>
    public void ValidateProductType(string productType)
    {
        if (string.IsNullOrWhiteSpace(productType))
        {
            return;
        }

        productType = productType.Trim();

        // ProductType mavjudligini tekshirish
        var existingType = CurrentProductEntry.AvailableProductTypes.FirstOrDefault(t =>
            t.Type?.Trim().Equals(productType, StringComparison.OrdinalIgnoreCase) == true);

        if (existingType is not null)
        {
            // Mavjud type - o'rnatish
            CurrentProductEntry.ProductType = existingType;
        }
        else
        {
            // Yangi type - tasdiqlash so'rash
            var result = MessageBox.Show(
                $"'{productType}' razmeri topilmadi. Yangi razmer qo'shmoqchimisiz?",
                "Tasdiqlash",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                // Foydalanuvchi rad etdi - type ni tozalash
                CurrentProductEntry.ProductType = null;
                return;
            }

            // Foydalanuvchi tasdiqladi - yangi type yaratish
            var newType = new ProductTypeViewModel
            {
                Type = productType,
                BundleItemCount = 0 // Default qiymat, foydalanuvchi keyinroq kiritadi
            };

            CurrentProductEntry.ProductType = newType;
        }
    }
    #endregion

    #region Commands

    [RelayCommand]
    private void Add()
    {
        if (CurrentProductEntry.Product == null || string.IsNullOrWhiteSpace(CurrentProductEntry.Product.Code))
        {
            ErrorMessage = "Mahsulot kodini kiriting!";
            return;
        }

        if (CurrentProductEntry.ProductType == null || string.IsNullOrWhiteSpace(CurrentProductEntry.ProductType.Type))
        {
            ErrorMessage = "Razmerni kiriting!";
            return;
        }

        if (CurrentProductEntry.BundleCount <= 0)
        {
            ErrorMessage = "Qop sonini kiriting!";
            return;
        }

        if (CurrentProductEntry.UnitPrice <= 0)
        {
            ErrorMessage = "Tannarxni kiriting!";
            return;
        }

        var item = new ProductEntryViewModel
        {
            Product = CurrentProductEntry.Product,
            ProductType = CurrentProductEntry.ProductType,
            BundleItemCount = CurrentProductEntry.BundleItemCount,
            BundleCount = CurrentProductEntry.BundleCount,
            TotalCount = CurrentProductEntry.TotalCount,
            UnitPrice = CurrentProductEntry.UnitPrice
        };

        ProductEntries.Add(item);

        // Formani tozalash
        ClearCurrentEntry();
    }

    [RelayCommand]
    private async Task Submit()
    {
        if (ProductEntries.Count == 0)
        {
            ErrorMessage = "Hech qanday mahsulot qo'shilmagan!";
            return;
        }

        // Bu yerda ma'lumotlarni serverga yuborish logikasi bo'ladi
        var result = MessageBox.Show(
            $"{ProductEntries.Count} ta mahsulotni saqlashni tasdiqlaysizmi?",
            "Tasdiqlash",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // Serverga yuborish
            // await SaveToServerAsync();

            SuccessMessage = "Ma'lumotlar muvaffaqiyatli saqlandi!";
            ProductEntries.Clear();
            ClearCurrentEntry();
        }
    }

    private void ClearCurrentEntry()
    {
        CurrentProductEntry.Product = null;
        CurrentProductEntry.ProductType = null;
        CurrentProductEntry.BundleCount =
        CurrentProductEntry.BundleItemCount =
        CurrentProductEntry.TotalCount = null;
        CurrentProductEntry.UnitPrice = null;
        CurrentProductEntry.AvailableProductTypes = [];
    }
    #endregion
}