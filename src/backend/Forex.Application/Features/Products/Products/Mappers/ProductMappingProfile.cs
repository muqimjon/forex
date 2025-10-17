namespace Forex.Application.Features.Products.Products.Mappers;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Features.Products.Products.Commands;
using Forex.Application.Features.Products.Products.DTOs;
using Forex.Domain.Entities.Products;

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

        CreateMap<ProductCommand, Product>();

        CreateMap<Product, ProductDto>();
        CreateMap<Product, ProductForProductTypeDto>();
    }
}
