namespace Forex.Wpf.Pages.Processes.ViewModels;

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
using System.Collections.Specialized;
using System.ComponentModel;

public partial class ProcessPageViewModel : ViewModelBase
{
    private readonly ForexClient Client;
    private readonly IMapper Mapper;

    public ProcessPageViewModel(ForexClient client, IMapper mapper)
    {
        Client = client;
        Mapper = mapper;

        EntryToProcessByProduct = [];
        EntryToProcessByProduct.CollectionChanged += EntryCollectionChanged;

        _ = InitialDate();
    }

    [ObservableProperty] private ObservableCollection<ProductViewModel> availableProducts = [];
    [ObservableProperty] private ObservableCollection<EntryToProcessByProductViewModel> entryToProcessByProduct;

    #region Commands

    [RelayCommand]
    public async Task Submit()
    {
        var entrys = Mapper.Map<List<EntryToProcessRequest>>(EntryToProcessByProduct);

        var response = await Client.Processes.CreateAsync(entrys)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            SuccessMessage = "Jarayon muvaffaqiyatli yaratildi.";
            EntryToProcessByProduct.Clear();
        }
        else ErrorMessage = response.Message ?? "Jarayon yaratishda xatolik yuz berdi.";
    }

    #endregion Commands

    #region Load Data

    private async Task InitialDate()
    {
        await LoadProductsAsync();
    }

    public async Task LoadProductsAsync()
    {
        var response = await Client.Products.GetAllAsync()
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
            AvailableProducts = Mapper.Map<ObservableCollection<ProductViewModel>>(response.Data);
        else WarningMessage = "Mahsulotlarni yuklashda xatolik.";
    }

    public async Task LoadTypesAsync(ProductViewModel product)
    {
        if (product is null || product.Id == 0)
        {
            ErrorMessage = "Mahsulot tanlanmagan!";
            return;
        }

        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["productid"] = [product.Id.ToString()],
                ["typeitem"] = ["include:semiproduct"]
            }
        };

        var response = await Client.ProductTypes.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
            product.ProductTypes = Mapper.Map<ObservableCollection<ProductTypeViewModel>>(response.Data);
        else ErrorMessage = response.Message ?? "Mahsulot o‘lchamlarini yuklashda xatolik!";
    }

    #endregion

    #region Tracking

    private void EntryCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (EntryToProcessByProductViewModel entry in e.NewItems)
                entry.PropertyChanged += Entry_PropertyChanged;
        }

        if (e.OldItems is not null)
        {
            foreach (EntryToProcessByProductViewModel entry in e.OldItems)
                entry.PropertyChanged -= Entry_PropertyChanged;
        }
    }

    private void Entry_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not EntryToProcessByProductViewModel entry) return;

        if (e.PropertyName == nameof(EntryToProcessByProductViewModel.Product))
        {
            _ = LoadTypesAsync(entry.Product);
        }
    }

    #endregion
}
