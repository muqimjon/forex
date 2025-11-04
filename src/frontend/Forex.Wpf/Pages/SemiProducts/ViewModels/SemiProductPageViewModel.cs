namespace Forex.Wpf.Pages.SemiProducts.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Models.Requests;
using Forex.Wpf.Pages.Common;
using Forex.Wpf.ViewModels;
using MapsterMapper;
using System.Collections.ObjectModel;

public partial class SemiProductPageViewModel : ViewModelBase
{
    private readonly ForexClient Client;
    private readonly IMapper Mapper;

    public SemiProductPageViewModel(ForexClient client, IMapper mapper)
    {
        Client = client;
        Mapper = mapper;
        Products.Add(new());
        Invoice = new(client, mapper);
    }

    [ObservableProperty] private InvoiceViewModel invoice;

    [ObservableProperty] private ObservableCollection<ProductViewModel> products = [];


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
            Invoice = Mapper.Map<InvoiceRequest>(Invoice),
            Products = Mapper.Map<ICollection<ProductRequest>>(Products)
        };

        var response = await Client.SemiProductEntry.Create(requestObject)
            .Handle(isLoading => IsLoading = isLoading);

        if (response.IsSuccess)
        {
            SuccessMessage = "Yarim tayyor mahsulot muvaffaqiyatli yuklandi.";

            Products.Clear();
        }
        else
            ErrorMessage = response.Message ?? "Yuklashda xatolik yuz berdi.";
    }

    #endregion
}