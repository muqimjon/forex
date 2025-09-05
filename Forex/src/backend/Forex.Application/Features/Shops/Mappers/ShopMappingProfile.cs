namespace Forex.Application.Features.Shops.Mappers;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Features.Shops.Commands;
using Forex.Application.Features.Shops.DTOs;
using Forex.Domain.Entities;

public class ShopMappingProfile : Profile
{
    public ShopMappingProfile()
    {
        CreateMap<CreateShopCommand, Shop>()
            .ForMember(dest => dest.NormalizedName,
                opt => opt.MapFrom(src => src.Name.ToNormalized()));

        CreateMap<UpdateShopCommand, Shop>()
            .ForMember(dest => dest.NormalizedName,
                opt => opt.MapFrom(src => src.Name.ToNormalized()));

        CreateMap<Shop, ShopDto>();
    }
}
