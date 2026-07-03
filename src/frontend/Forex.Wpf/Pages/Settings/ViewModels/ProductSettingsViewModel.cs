namespace Forex.Wpf.Pages.Settings.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Common.Interfaces;
using Forex.Wpf.Common.Services;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using Mapster;
using MapsterMapper;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

public partial class ProductSettingsViewModel : ViewModelBase
{
    private readonly ForexClient client;
    private readonly IMapper mapper;
    private readonly IDialogService dialogService;

    public ProductSettingsViewModel(ForexClient client, IMapper mapper, IDialogService dialogService)
    {
        this.client = client;
        this.mapper = mapper;
        this.dialogService = dialogService;
        _ = LoadDataAsync();
    }

    [ObservableProperty]
    private ObservableCollection<ProductViewModel> products = [];

    [ObservableProperty]
    private ObservableCollection<UnitMeasuerViewModel> unitMeasures = [];

    [ObservableProperty]
    private ProductViewModel? selectedProduct;

    [ObservableProperty]
    private string searchText = string.Empty;

    private ICollectionView? _productsView;
    public ICollectionView? ProductsView
    {
        get => _productsView;
        set => SetProperty(ref _productsView, value);
    }

    public bool HasSelectedProduct => SelectedProduct is not null;

    partial void OnSelectedProductChanged(ProductViewModel? value)
    {
        OnPropertyChanged(nameof(HasSelectedProduct));
    }

    partial void OnSearchTextChanged(string value)
    {
        ProductsView?.Refresh();
    }

    private async Task LoadDataAsync()
    {
        await Task.WhenAll(LoadProductsAsync(), LoadUnitMeasuresAsync());
    }

    private async Task LoadUnitMeasuresAsync()
    {
        var response = await client.UnitMeasures.GetAllAsync();
        if (response.IsSuccess)
            UnitMeasures = mapper.Map<ObservableCollection<UnitMeasuerViewModel>>(response.Data);
    }

    private async Task LoadProductsAsync()
    {
        var response = await client.Products.GetAllAsync().Handle(l => IsLoading = l);
        if (response.IsSuccess)
        {
            Products = mapper.Map<ObservableCollection<ProductViewModel>>(response.Data);

            ProductsView = CollectionViewSource.GetDefaultView(Products);
            ProductsView.Filter = FilterProducts;
        }
        else
            ErrorMessage = response.Message ?? "Mahsulotlarni yuklashda xatolik!";
    }

    private bool FilterProducts(object obj)
    {
        if (obj is not ProductViewModel product)
            return false;

        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        var search = SearchText.Trim();

        return TransliterationHelper.ContainsIgnoreScript(product.Code ?? string.Empty, search) ||
               TransliterationHelper.ContainsIgnoreScript(product.Name ?? string.Empty, search);
    }

    [RelayCommand]
    private void AddProduct()
    {
        var newProduct = new ProductViewModel { Name = "Yangi mahsulot", Code = "" };

        if (UnitMeasures.Any())
        {
            var defaultUnit = UnitMeasures.First();
            newProduct.UnitMeasure = defaultUnit;
            newProduct.UnitMeasureId = defaultUnit.Id;
        }

        Products.Insert(0, newProduct);
        SelectedProduct = newProduct;
    }

    [RelayCommand]
    private async Task SaveProduct()
    {
        if (SelectedProduct is null) return;

        if (string.IsNullOrWhiteSpace(SelectedProduct.Code))
        {
            WarningMessage = "Mahsulot kodi kiritilmagan!";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedProduct.Name))
        {
            WarningMessage = "Mahsulot nomi kiritilmagan!";
            return;
        }

        var request = mapper.Map<ProductRequest>(SelectedProduct);

        if (SelectedProduct.ProductTypes.Count > 0)
            request.ProductTypes = SelectedProduct.ProductTypes.Adapt<List<ProductTypeRequest>>();

        if (SelectedProduct.Id > 0)
        {
            var response = await client.Products.Update(request).Handle(l => IsLoading = l);
            if (response.IsSuccess)
            {
                foreach (var type in SelectedProduct.ProductTypes)
                    type.IsNew = false;

                SuccessMessage = "Mahsulot muvaffaqiyatli yangilandi!";
            }
            else
                ErrorMessage = response.Message ?? "Yangilashda xatolik!";
        }
        else
        {
            var response = await client.Products.Create(request).Handle(l => IsLoading = l);
            if (response.IsSuccess)
            {
                SelectedProduct.Id = response.Data ?? 0;

                foreach (var type in SelectedProduct.ProductTypes)
                    type.IsNew = false;

                SuccessMessage = "Mahsulot muvaffaqiyatli saqlandi!";
            }
            else
            {
                ErrorMessage = response.Message ?? "Saqlashda xatolik!";
            }
        }
    }

    [RelayCommand]
    private async Task DeleteProduct()
    {
        if (SelectedProduct is null) return;
        if (SelectedProduct.Id <= 0)
        {
            Products.Remove(SelectedProduct);
            SelectedProduct = null;
            return;
        }

        if (!await dialogService.ShowYesNoAsync($"'{SelectedProduct.Name}' mahsulotini o'chirmoqchimisiz?"))
            return;

        var response = await client.Products.Delete(SelectedProduct.Id).Handle(l => IsLoading = l);

        if (response.IsSuccess)
        {
            Products.Remove(SelectedProduct);
            SelectedProduct = null;
            SuccessMessage = "Mahsulot muvaffaqiyatli o'chirildi!";
        }
        else
        {
            WarningMessage = response.Message ?? "O'chirishda xatolik! Mahsulot savdoda qatnashgan bo'lishi mumkin.";
        }
    }

    [RelayCommand]
    private void AddProductType()
    {
        if (SelectedProduct is null) return;

        var existingNewType = SelectedProduct.ProductTypes
            .FirstOrDefault(t => string.IsNullOrWhiteSpace(t.Type));

        if (existingNewType is not null)
        {
            WarningMessage = "Avval mavjud turni to'ldiring!";
            return;
        }

        var newType = new ProductTypeViewModel
        {
            Type = "",
            BundleItemCount = 1,
            UnitPrice = 0,
            ProductId = SelectedProduct.Id,
            IsNew = true
        };

        SelectedProduct.ProductTypes.Add(newType);
    }

    [RelayCommand]
    private async Task DeleteProductType(ProductTypeViewModel? type)
    {
        if (type is null || SelectedProduct is null) return;

        if (!await dialogService.ShowYesNoAsync($"'{type.Type}' turini o'chirmoqchimisiz?"))
            return;

        if (type.Id <= 0)
        {
            SelectedProduct.ProductTypes.Remove(type);
            return;
        }

        var response = await client.ProductTypes.Delete(type.Id).Handle(l => IsLoading = l);

        if (response.IsSuccess)
        {
            SelectedProduct.ProductTypes.Remove(type);
            SuccessMessage = "Tur muvaffaqiyatli o'chirildi!";
        }
        else
        {
            WarningMessage = response.Message ?? "Turni o'chirishda xatolik! U savdoda qatnashgan bo'lishi mumkin.";
        }
    }

    [RelayCommand]
    private async Task GenerateAllBarcodes()
    {
        var response = await client.ProductTypes.GenerateBarcodes().Handle(l => IsLoading = l);
        if (response.IsSuccess)
        {
            await LoadProductsAsync();
            SuccessMessage = $"{response.Data} ta razmerga barkod yaratildi.";
        }
        else
            ErrorMessage = response.Message ?? "Barkod yaratishda xatolik!";
    }

    [RelayCommand]
    private void PrintLabels(ProductTypeViewModel? type)
    {
        if (type is null || SelectedProduct is null) return;

        if (string.IsNullOrWhiteSpace(type.QopBarcode) && string.IsNullOrWhiteSpace(type.PackBarcode))
        {
            WarningMessage = "Avval 'Barkod yaratish' tugmasini bosing.";
            return;
        }

        var title = $"{SelectedProduct.Code} {SelectedProduct.Name}".Trim();

        // Barkod bo'limidagi kabi modal — razmerni tanlab, nusxa soni bilan chop etish.
        var vm = new Forex.Wpf.Windows.BarcodeLabelPreviewViewModel(
            title, type.Type,
            type.BundleItemCount ?? 0, type.PackItemCount ?? 0,
            type.QopBarcode, type.PackBarcode, SelectedProduct.DisplayImagePath);

        new Forex.Wpf.Windows.BarcodePreviewWindow(vm)
        {
            Owner = System.Windows.Application.Current.MainWindow
        }.ShowDialog();
    }

    [RelayCommand]
    private async Task UploadImage()
    {
        if (SelectedProduct is null) return;

        var dialog = new OpenFileDialog
        {
            Filter = "Rasmlar (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
            Title = "Mahsulot rasmi tanlash"
        };

        if (dialog.ShowDialog() == true)
        {
            // Darhol lokal fayldan ko'rsatamiz
            SelectedProduct.ImagePreviewPath = dialog.FileName;

            // Force UI update
            var tempProduct = SelectedProduct;
            SelectedProduct = null;
            SelectedProduct = tempProduct;

            // Background'da MinIO'ga yuklaymiz
            var uploadedPath = await client.FileStorage.UploadFileAsync(dialog.FileName);

            if (!string.IsNullOrEmpty(uploadedPath))
            {
                SelectedProduct.ImagePath = uploadedPath;
                InfoMessage = "Rasm yuklandi! Saqlash tugmasini bosishni unutmang.";
            }
            else
            {
                ErrorMessage = "Rasm yuklashda xatolik!";
                SelectedProduct.ImagePreviewPath = string.Empty;
            }
        }
    }

    [RelayCommand]
    private void DeleteImage()
    {
        if (SelectedProduct is null) return;

        SelectedProduct.ImagePath = string.Empty;
        InfoMessage = "Rasm o'chirildi! Saqlash tugmasini bosishni unutmang.";
    }
}
