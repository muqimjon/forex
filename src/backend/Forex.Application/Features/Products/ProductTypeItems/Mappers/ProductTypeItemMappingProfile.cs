namespace Forex.Application.Features.Products.ProductTypeItems.Mappers;

using AutoMapper;
using Forex.Application.Features.Products.ProductTypeItems.Commands;
using Forex.Application.Features.Products.ProductTypeItems.DTOs;
using Forex.Domain.Entities.Products;

public class ProductTypeItemMappingProfile : Profile
{
    public ProductTypeItemMappingProfile()
    {
        CreateMap<ProductTypeItem, ProductTypeItemDto>();
        CreateMap<ProductTypeItem, ProductTypeItemForProductTypeDto>();
        CreateMap<ProductTypeItem, ProductTypeItemForSemiProductDto>();

        CreateMap<ProductTypeItemCommand, ProductTypeItem>();
    }
}
