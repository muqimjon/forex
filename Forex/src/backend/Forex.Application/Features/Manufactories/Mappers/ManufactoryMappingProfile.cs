namespace Forex.Application.Features.Manufactories.Mappers;

using AutoMapper;
using Forex.Application.Features.Manufactories.Commands;
using Forex.Application.Features.Manufactories.DTOs;
using Forex.Application.Features.ShopCashes.Commands;
using Forex.Domain.Entities;

public class ManufactoryMappingProfile : Profile
{
    public ManufactoryMappingProfile()
    {
        CreateMap<CreateShopCashCommand, Manufactory>();
        CreateMap<UpdateManufactoryCommand, Manufactory>();
        CreateMap<Manufactory, ManufactoryDto>();
    }
}
