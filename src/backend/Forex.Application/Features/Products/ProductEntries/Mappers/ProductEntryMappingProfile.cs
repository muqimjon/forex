namespace Forex.Application.Features.Products.ProductEntries.Mappers;

using AutoMapper;
using Forex.Application.Features.Products.ProductEntries.DTOs;
using Forex.Domain.Entities.Products;

public class ProductEntryMappingProfile : Profile
{
    public ProductEntryMappingProfile()
    {
        CreateMap<ProductEntry, ProductEntryDto>();
        CreateMap<ProductEntry, ProductEntryForProductTypeDto>();
        CreateMap<ProductEntry, ProductEntryForShopDto>();
        CreateMap<ProductEntry, ProductEntryForProductResidueDto>();
    }
}
