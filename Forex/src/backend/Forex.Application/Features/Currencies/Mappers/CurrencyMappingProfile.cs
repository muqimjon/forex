namespace Forex.Application.Features.Currencies.Mappers;

using AutoMapper;
using Forex.Application.Features.Currencies.Commands;
using Forex.Application.Features.Currencies.DTOs;
using Forex.Application.Features.ShopCashes.Commands;
using Forex.Domain.Entities;

public class CurrencyMappingProfile : Profile
{
    public CurrencyMappingProfile()
    {
        CreateMap<CreateShopCashCommand, Currency>();
        CreateMap<UpdateCurrencyCommand, Currency>();
        CreateMap<Currency, CurrencyDto>();
    }
}
