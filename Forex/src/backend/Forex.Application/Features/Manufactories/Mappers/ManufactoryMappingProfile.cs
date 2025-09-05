namespace Forex.Application.Features.Manufactories.Mappers;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Features.Manufactories.Commands;
using Forex.Application.Features.Manufactories.DTOs;
using Forex.Domain.Entities.Manufactories;

public class ManufactoryMappingProfile : Profile
{
    public ManufactoryMappingProfile()
    {
        CreateMap<CreateManufactoryCommand, Manufactory>()
            .ForMember(dest => dest.NormalizedName,
            opt => opt.MapFrom(src => src.Name.ToNormalized()));

        CreateMap<UpdateManufactoryCommand, Manufactory>()
            .ForMember(dest => dest.NormalizedName,
            opt => opt.MapFrom(src => src.Name.ToNormalized()));

        CreateMap<Manufactory, ManufactoryDto>();
    }
}
