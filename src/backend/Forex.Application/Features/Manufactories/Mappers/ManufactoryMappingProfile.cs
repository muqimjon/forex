namespace Forex.Application.Features.Manufactories.Mappers;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Features.Manufactories.Commands;
using Forex.Application.Features.Manufactories.DTOs;
using Forex.Application.Features.SemiProducts.SemiProductResidues.DTOs;
using Forex.Domain.Entities;
using Forex.Domain.Entities.SemiProducts;

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
        CreateMap<Manufactory, ManufactoryForSemiProductEntryDto>();
        CreateMap<Manufactory, ManufactoryForSemiProductResidueDto>();
        CreateMap<SemiProductResidue, SemiProductResidueDto>();
    }
}
