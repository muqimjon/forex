namespace Forex.Application.Features.SemiProducts.SemiProductEntries.Mappers;

using AutoMapper;
using Forex.Domain.Entities;
using Forex.Domain.Entities.SemiProducts;
using Forex.Domain.Entities.Products;
using Forex.Application.Features.SemiProducts.SemiProductEntries.DTOs;

public class SemiProductMappingProfile : Profile
{
    public SemiProductMappingProfile()
    {
        // Invoice
        CreateMap<InvoiceCommand, Invoice>();

        // SemiProduct
        CreateMap<SemiProductCommand, SemiProduct>()
            .ForMember(dest => dest.ImagePath, opt => opt.Ignore()); // Fayl yuklanadi, mapping emas

        // Product
        CreateMap<ProductCommand, Product>()
            .ForMember(dest => dest.ImagePath, opt => opt.Ignore()) // Fayl yuklanadi
            .ForMember(dest => dest.ProductTypes, opt => opt.MapFrom(src => src.Types));

        // ProductType
        CreateMap<ProductTypeCommand, ProductType>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        // ProductTypeItem
        CreateMap<ProductTypeItemCommand, ProductTypeItem>()
            .ForMember(dest => dest.SemiProduct, opt => opt.Ignore()); // Code orqali bog‘lanadi

        // Supplier / Sender
        CreateMap<UserCommand, User>();
    }
}
