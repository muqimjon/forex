namespace Forex.Application.Features.SemiProducts.SemiProductResidues.Mappers;

using AutoMapper;
using Forex.Application.Features.SemiProducts.SemiProductResidues.DTOs;
using Forex.Domain.Entities.SemiProducts;

public class SemiProductResidueMappingProfile : Profile
{
    public SemiProductResidueMappingProfile()
    {
        CreateMap<SemiProductResidue, SemiProductResidueDto>();
        CreateMap<SemiProductResidue, SemiProductResidueForManufactoryDto>();
        CreateMap<SemiProductResidue, SemiProductResidueForSemiProdutDto>();
    }
}
