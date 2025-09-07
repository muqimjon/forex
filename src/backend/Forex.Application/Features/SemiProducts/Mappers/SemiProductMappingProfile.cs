namespace Forex.Application.Features.SemiProducts.Mappers;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Features.SemiProducts.Commands;
using Forex.Application.Features.SemiProducts.DTOs;
using Forex.Domain.Entities.Manufactories;

public class SemiProductMappingProfile : Profile
{
    public SemiProductMappingProfile()
    {
        CreateMap<SemiProductCommand, SemiProduct>()
            .ForMember(dest => dest.NormalizedName,
                opt => opt.MapFrom(src => src.Name!.ToNormalized()));

        CreateMap<UpdateSemiProductCommand, SemiProduct>()
            .ForMember(dest => dest.NormalizedName,
                opt => opt.MapFrom(src => src.Name!.ToNormalized()));

        CreateMap<SemiProduct, SemiProductDto>();
    }
}
