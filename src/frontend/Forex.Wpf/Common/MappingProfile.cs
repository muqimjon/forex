namespace Forex.Wpf.Common;

using AutoMapper;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Pages.SemiProducts.ViewModels;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product
        CreateMap<ProductResponse, ProductViewModel>();
        CreateMap<ProductViewModel, ProductRequest>()
            .ForMember(dest => dest.MeasureId,
                       opt => opt.MapFrom(src => src.Measure.Id));

        // Product Type
        CreateMap<ProductTypeResponse, ProductTypeViewModel>();
        CreateMap<ProductTypeViewModel, ProductTypeRequest>();

        // Type Item
        CreateMap<ProductTypeItemViewModel, ProductTypeItemRequest>();

        // SemiProduct
        CreateMap<SemiProductResponse, SemiProductViewModel>();
        CreateMap<SemiProductViewModel, SemiProductRequest>()
            .ForMember(dest => dest.MeasureId,
                       opt => opt.MapFrom(src => src.Measure.Id));

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

        // Invoice
        CreateMap<InvoiceResponse, InvoiceViewModel>();
        CreateMap<InvoiceViewModel, InvoiceRequest>()
            .ForMember(dest => dest.CurrencyId,
                       opt => opt.MapFrom(src => src.Currency.Id))
            .ForMember(dest => dest.SupplierId,
                       opt => opt.MapFrom(src => src.Supplier.Id))
            .ForMember(dest => dest.SenderId,
                        opt => opt.MapFrom(src => src.Agent != null ? src.Agent.Id : (long?)null))
            .ForMember(dest => dest.ManufactoryId,
                       opt => opt.MapFrom(src => src.Manufactory.Id));
    }
}