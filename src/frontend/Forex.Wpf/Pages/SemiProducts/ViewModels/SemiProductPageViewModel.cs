namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService.Enums;
using Forex.ClientService.Extensions;
using Forex.ClientService.Interfaces;
using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

public partial class SemiProductPageViewModel : ViewModelBase
{
    private readonly IServiceProvider services;
    private readonly IMapper mapper;

    public SemiProductPageViewModel(IServiceProvider services, IMapper mapper)
    {
        this.services = services;
        this.mapper = mapper;
        Products.Add(new());

        _ = LoadDataAsync();
    }

    [ObservableProperty] private InvoiceViewModel invoice = new();

    [ObservableProperty] private ObservableCollection<ProductViewModel> products = [];
    [ObservableProperty] private ObservableCollection<UnitMeasuerViewModel> availableUnitMeasures = [];
    [ObservableProperty] private ObservableCollection<UserViewModel> availableSuppliers = [];
    [ObservableProperty] private ObservableCollection<UserViewModel> availableAgents = [];


    #region Load Data

    private async Task LoadDataAsync()
    {
        await LoadUnitMeasures();
        await LoadUsersAsync();
    }

    private async Task LoadUnitMeasures()
    {
        var client = services.GetRequiredService<IApiUnitMeasures>();
        var response = await client.GetAllAsync().Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
            AvailableUnitMeasures = mapper.Map<ObservableCollection<UnitMeasuerViewModel>>(response.Data);
        else ErrorMessage = response.Message ?? "O'lchov birliklarini yuklashda xatolik";
    }

    private async Task LoadUsersAsync()
    {
        FilteringRequest request = new()
        {
            Filters = new()
            {
                ["role"] = ["in:Taminotchi,Vositachi"]
            }
        };

        var client = services.GetRequiredService<IApiUser>();
        var response = await client.Filter(request)
            .Handle(isLoading => IsLoading = isLoading);

        if (!response.IsSuccess)
        {
            ErrorMessage = response.Message ?? "Foydalanuvchilarni yuklashda noma'lum xatolik yuz berdi.";
            return;
        }

        AvailableSuppliers = mapper.Map<ObservableCollection<UserViewModel>>(response.Data!.Where(u => u.Role == UserRole.Taminotchi));
        AvailableAgents = mapper.Map<ObservableCollection<UserViewModel>>(response.Data!.Where(u => u.Role == UserRole.Vositachi));

        Invoice.Supplier = AvailableSuppliers.FirstOrDefault() ?? new();
    }

    #endregion Load Data

    #region Commands

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (Products.Count == 0)
        {
            ErrorMessage = "Hech qanday yarim tayyor mahsulot kiritilmadi.";
            return;
        }

        var requestObject = new SemiProductIntakeRequest
        {
            Invoice = mapper.Map<InvoiceRequest>(Invoice),
            Products = mapper.Map<ICollection<ProductRequest>>(Products)
        };

        var client = services.GetRequiredService<IApiSemiProductEntry>();

        var response = await client.Create(requestObject)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            SuccessMessage = "Yarim tayyor mahsulot muvaffaqiyatli yuklandi.";
            Products.Clear();
        }
        else ErrorMessage = response.Message ?? "Yuklashda xatolik yuz berdi.";
    }

    #endregion
}