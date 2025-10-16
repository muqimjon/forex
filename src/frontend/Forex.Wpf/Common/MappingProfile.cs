namespace Forex.Wpf.Common;

using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Forex.Wpf.Pages.SemiProducts.ViewModels;
using Forex.Wpf.ViewModels;
using Mapster;

public static class MappingProfile
{
    public static void Register(TypeAdapterConfig config)
    {
        // Product
        config.NewConfig<ProductResponse, ProductViewModel>();
        config.NewConfig<ProductViewModel, ProductRequest>()
            .Map(dest => dest.UnitMeasureId, src => src.Measure.Id);

        // Product Type
        config.NewConfig<ProductTypeResponse, ProductTypeViewModel>();
        config.NewConfig<ProductTypeViewModel, ProductTypeRequest>();

        // Type Item
        config.NewConfig<ProductTypeItemViewModel, ProductTypeItemRequest>();

        // SemiProduct
        config.NewConfig<SemiProductResponse, SemiProductViewModel>();
        config.NewConfig<SemiProductViewModel, SemiProductRequest>()
            .Map(dest => dest.UnitMeasureId, src => src.Measure.Id);

        // UnitMeasure
        config.NewConfig<UnitMeasureResponse, UnitMeasuerViewModel>();
        config.NewConfig<UnitMeasuerViewModel, UnitMeasureRequest>();

        // User
        config.NewConfig<UserResponse, UserViewModel>();
        config.NewConfig<UserViewModel, UserRequest>();

        // Currency
        config.NewConfig<CurrencyResponse, CurrencyViewModel>();
        config.NewConfig<CurrencyViewModel, CurrencyRequest>();

        // Manufactory
        config.NewConfig<ManufactoryResponse, ManufactoryViewModel>();

        // Invoice
        config.NewConfig<InvoiceResponse, InvoiceViewModel>();
        config.NewConfig<InvoiceViewModel, InvoiceRequest>()
            .Map(dest => dest.CurrencyId, src => src.Currency.Id)
            .Map(dest => dest.SupplierId, src => src.Supplier.Id)
            .Map(dest => dest.SenderId, src => src.Agent != null ? src.Agent.Id : (long?)null)
            .Map(dest => dest.ManufactoryId, src => src.Manufactory.Id);
    }
}
