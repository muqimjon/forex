namespace Forex.Wpf.Pages.Processes.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Commons;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public partial class ProcessPageViewModel : ViewModelBase
{
    private readonly ForexClient Client;
    private readonly IMapper Mapper;
    public ProcessPageViewModel(ForexClient client, IMapper mapper)
    {
        Client = client;
        Mapper = mapper;

        _ = InitialDate();
    }

    private async Task InitialDate()
    {
        await LoadProductsAsync();
    }

    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];

    [ObservableProperty] private ObservableCollection<ProductEntryViewModel> filteredProducts = [];
    [ObservableProperty] private ProductEntryViewModel? selectedProduct;

    [RelayCommand]
    private void AddProduct()
    {
        ProductEntryViewModel entry = new();
        FilteredProducts.Add(entry);
    }

    public async Task LoadProductsAsync()
    {

        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["unitMeasure"] = ["include"]
            }
        };

        var response = await Client.Products.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
            AvailableProducts = Mapper.Map<ObservableCollection<ProductViewModel>>(response.Data);
        else 
            WarningMessage = "Mahsulotlarni yuklashda xatolik.";
    }

    partial void OnSelectedProductChanged(ProductEntryViewModel? value)
    {
        if (value != null && value.Product != null)
        {
            _ = LoadTypesAsync(value);
        }
    }

    private async Task LoadTypesAsync(ProductEntryViewModel model)
    {
        if (model?.Product == null)
        {
            ErrorMessage = "Mahsulot tanlanmagan!";
            return;
        }

        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["productid"] = [model.Product.Id.ToString()],
                ["productResidue"] = ["include"]
            }
        };

        var response = await Client.ProductTypes.Filter(request).Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess && response.Data != null)
            model.AvailableProductTypes = Mapper.Map<ObservableCollection<ProductTypeViewModel>>(response.Data);
        else
            ErrorMessage = response.Message ?? "Mahsulot o‘lchamlarini yuklashda xatolik!";

    }

    private async Task LoadTypeItemsAsync(ProductEntryViewModel model)
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["productTypeId"] = [model.ProductType.Id.ToString()],
                ["semiproduct"] = ["include:unitMeasure"]
            }
        };

        var response = await Client.ProductTypeItems.Filter(request).Handle(isLoading => IsLoading = isLoading);
        if (response.IsSuccess)
            model.ProductType.ProductTypeItems = Mapper.Map<ObservableCollection<ProductTypeItemViewModel>>(response.Data);
        else ErrorMessage = response.Message ?? "Yarim tayyor mahsulotlarni yuklashda xatolik!";
    }
}
