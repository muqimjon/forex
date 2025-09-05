namespace Forex.Application.Features.ShopCashes.Mappers;

using AutoMapper;
using Forex.Application.Features.ShopCashes.Commands;
using Forex.Application.Features.ShopCashes.DTOs;
using Forex.Domain.Entities.Shops;

public class ShopCashMappingProfile : Profile
{
    public ShopCashMappingProfile()
    {
        CreateMap<CreateShopCashCommand, ShopCash>();
        CreateMap<UpdateShopCashCommand, ShopCash>();
        CreateMap<ShopCash, ShopCashDto>();
    }
}
