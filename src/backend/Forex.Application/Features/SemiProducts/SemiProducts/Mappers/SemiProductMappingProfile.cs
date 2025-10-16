namespace Forex.Application.Features.SemiProducts.SemiProducts.Mappers;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Features.SemiProducts.SemiProducts.Commands;
using Forex.Application.Features.SemiProducts.SemiProducts.DTOs;
using Forex.Domain.Entities.SemiProducts;

public class SemiProductMappingProfile : Profile
{
    public SemiProductMappingProfile()
    {
        CreateMap<CreateSemiProductCommand, SemiProduct>()
            .ForMember(dest => dest.NormalizedName,
                opt => opt.MapFrom(src => src.Name!.ToNormalized()));

        CreateMap<UpdateSemiProductCommand, SemiProduct>()
            .ForMember(dest => dest.NormalizedName,
                opt => opt.MapFrom(src => src.Name!.ToNormalized()));

        CreateMap<SemiProduct, SemiProductDto>();
    }
}
