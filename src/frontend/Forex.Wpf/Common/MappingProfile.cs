namespace Forex.Wpf.Common;

using AutoMapper;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Pages.SemiProducts.ViewModels;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // SemiProduct
        CreateMap<SemiProductResponse, SemiProductViewModel>();
        CreateMap<SemiProductViewModel, SemiProductRequest>();

        // UnitMeasure
        CreateMap<UnitMeasureResponse, UnitMeasuerViewModel>();
        CreateMap<UnitMeasuerViewModel, UnitMeasureRequest>();

        // User
        CreateMap<UserResponse, UserViewModel>();
        CreateMap<UserViewModel, UserRequest>();

        // Currency
        CreateMap<CurrencyResponse, CurrencyViewModel>();
        CreateMap<CurrencyViewModel, CurrencyRequest>();

        // Manufactory
        CreateMap<ManufactoryResponse, ManufactoryViewModel>();
    }
}