namespace Forex.Wpf.Pages.Settings.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService.Extensions;
using Forex.ClientService.Interfaces;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

public partial class SettingsPageViewModel : ViewModelBase
{
    private readonly IMapper mapper;
    private readonly IServiceProvider services;

    public SettingsPageViewModel(IServiceProvider services)
    {
        this.services = services;
        mapper = services.GetRequiredService<IMapper>();

        _ = LoadData();
    }

    [ObservableProperty] private ObservableCollection<CurrencyViewModel> currencies = [];
    [ObservableProperty] private ObservableCollection<UnitMeasuerViewModel> unitMeasures = [];

    #region Commands

    [RelayCommand]
    private void AddCurrency()
    {
        Currencies.Add(new());
    }

    [RelayCommand]
    private void AddUnitMeasure()
    {
        UnitMeasures.Add(new());
    }

    [RelayCommand]
    private void RemoveCurrency(CurrencyViewModel currency)
    {
        Currencies.Remove(currency);
    }

    [RelayCommand]
    private void RemoveUnitMeasure(UnitMeasuerViewModel unitMeasure)
    {
        UnitMeasures.Remove(unitMeasure);
    }

    [RelayCommand]
    private async Task SaveCurrencies()
    {
        if (Currencies is null || Currencies.Count == 0)
        {
            WarningMessage = "Saqlash uchun valyuta yo‘q";
            return;
        }

        var client = services.GetRequiredService<IApiCurrency>();
        var dtoList = mapper.Map<List<CurrencyRequest>>(Currencies);

        var response = await client.SaveAllAsync(dtoList)
            .Handle(isLoading => IsSelected = isLoading);

        if (response.IsSuccess) SuccessMessage = "O'zgarishlar muvaffaqiyatli saqlandi";
        else ErrorMessage = response.Message ?? "Valyutalarni saqlashda xatolik";
    }

    [RelayCommand]
    private async Task SaveUnitMeasures()
    {
        if (UnitMeasures is null || UnitMeasures.Count == 0)
        {
            WarningMessage = "Saqlash uchun valyuta yo‘q";
            return;
        }

        var client = services.GetRequiredService<IApiUnitMeasures>();
        var dtoList = mapper.Map<List<UnitMeasureRequest>>(UnitMeasures);

        var response = await client.SaveAllAsync(dtoList)
            .Handle(isLoading => IsSelected = isLoading);

        if (response.IsSuccess) SuccessMessage = "O'zgarishlar muvaffaqiyatli saqlandi";
        else ErrorMessage = response.Message ?? "O'lchov birliklarini saqlashda xatolik";
    }

    #endregion Commands

    #region Load Data

    private async Task LoadData()
    {
        await LoadCurrencies();
        await LoadUnitMeasures();
    }

    private async Task LoadUnitMeasures()
    {
        var client = services.GetRequiredService<IApiUnitMeasures>();
        var response = await client.GetAllAsync().Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
            UnitMeasures = mapper.Map<ObservableCollection<UnitMeasuerViewModel>>(response.Data);
        else ErrorMessage = response.Message ?? "O'lchov birliklarini yuklashda xatolik";
    }

    private async Task LoadCurrencies()
    {
        var client = services.GetRequiredService<IApiCurrency>();
        var response = await client.GetAllAsync().Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
            Currencies = mapper.Map<ObservableCollection<CurrencyViewModel>>(response.Data);
        else ErrorMessage = response.Message ?? "Valyutalarni yuklashda xatolik";
    }

    #endregion Load Data
}
