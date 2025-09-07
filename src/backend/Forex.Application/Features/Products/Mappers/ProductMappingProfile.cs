namespace Forex.Application.Features.Products.Mappers;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Features.Products.Commands;
using Forex.Application.Features.Products.DTOs;
using Forex.Domain.Entities.Shops;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<CreateProductCommand, Product>()
            .ForMember(dest => dest.NormalizedName,
                opt => opt.MapFrom(src => src.Name.ToNormalized()));

        CreateMap<UpdateProductCommand, Product>()
            .ForMember(dest => dest.NormalizedName,
                opt => opt.MapFrom(src => src.Name.ToNormalized()));

        CreateMap<Product, ProductDto>();
    }
}
