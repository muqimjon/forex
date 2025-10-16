namespace Forex.Application.Features.Products.ProductResidues.Mappers;

using AutoMapper;
using Forex.Application.Features.Products.ProductResidues.DTOs;
using Forex.Domain.Entities.Products;

public class ProductResidueMappingProfile : Profile
{
    public ProductResidueMappingProfile()
    {
        CreateMap<ProductResidue, ProductResidueDto>();
    }
}
