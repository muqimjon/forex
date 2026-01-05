namespace Forex.Application.Features.SemiProducts.SemiProducts.Mappers;

using AutoMapper;
using Forex.Application.Common.Extensions;
using Forex.Application.Features.SemiProducts.SemiProducts.Commands;
using Forex.Application.Features.SemiProducts.SemiProducts.DTOs;
using Forex.Domain.Entities.SemiProducts;

public class SemiProductMappingProfile : Profile
{
    public SemiProductMappingProfile()
    {
        CreateMap<SemiProductCommand, SemiProduct>()
            .ForMember(dest => dest.NormalizedName,
                opt => opt.MapFrom(src => src.Name!.ToNormalized()));

        CreateMap<SemiProduct, SemiProductDto>();
        CreateMap<SemiProduct, SemiProductForProductTypeItemDto>();
        CreateMap<SemiProduct, SemiProductForSemiProductEntryDto>();
        CreateMap<SemiProduct, SemiProductForSemiProductResidueDto>();
    }
}
