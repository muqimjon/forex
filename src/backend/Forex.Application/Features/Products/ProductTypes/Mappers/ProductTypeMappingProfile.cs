namespace Forex.Application.Features.Products.ProductTypes.Mappers;

using AutoMapper;
using Forex.Application.Features.Products.ProductTypes.DTOs;
using Forex.Domain.Entities.Products;

public class ProductTypeMappingProfile : Profile
{
    public ProductTypeMappingProfile()
    {
        CreateMap<ProductType, ProductTypeDto>();
    }
}
