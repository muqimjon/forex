namespace Forex.Application.Features.UnitMeasures.Mappers;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Features.UnitMeasures.Commands;
using Forex.Application.Features.UnitMeasures.DTOs;
using Forex.Domain.Entities;

public class UnitMeasureMappingProfile : Profile
{
    public UnitMeasureMappingProfile()
    {
        CreateMap<UnitMeasureCommand, UnitMeasure>()
            .ForMember(dest => dest.NormalizedName,
            opt => opt.MapFrom(src => src.Name.ToNormalized()));

        CreateMap<CreateUnitMeasureCommand, UnitMeasure>()
            .ForMember(dest => dest.NormalizedName,
            opt => opt.MapFrom(src => src.Name.ToNormalized()));

        CreateMap<UpdateUnitMeasureCommand, UnitMeasure>()
            .ForMember(dest => dest.NormalizedName,
            opt => opt.MapFrom(src => src.Name.ToNormalized()));

        CreateMap<UnitMeasure, UnitMeasureDto>();
    }
}
