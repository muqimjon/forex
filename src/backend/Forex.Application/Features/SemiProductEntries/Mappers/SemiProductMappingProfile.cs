namespace Forex.Application.Features.SemiProductEntries.Mappers;

using AutoMapper;
using Forex.Application.Features.SemiProductEntries.DTOs;
using Forex.Domain.Entities.Manufactories;

public class SemiProductMappingProfile : Profile
{
    public SemiProductMappingProfile()
    {
        CreateMap<ItemDto, SemiProduct>()
            .ForMember(dest => dest.NormalizedName,
                opt => opt.MapFrom(src => src.Name!.Trim().ToUpperInvariant()));

        CreateMap<ItemDto, SemiProductEntry>()
            .ForMember(dest => dest.SemiProductId, opt => opt.Ignore())
            .ForMember(dest => dest.InvoceId, opt => opt.Ignore())
            .ForMember(dest => dest.ManufactoryId, opt => opt.Ignore());
    }
}
