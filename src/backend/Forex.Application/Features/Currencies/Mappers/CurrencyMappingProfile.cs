namespace Forex.Application.Features.Currencies.Mappers;

using AutoMapper;
using Forex.Application.Common.Extensions;
using Forex.Application.Features.Currencies.Commands;
using Forex.Application.Features.Currencies.DTOs;
using Forex.Domain.Entities;

public class CurrencyMappingProfile : Profile
{
    public CurrencyMappingProfile()
    {
        CreateMap<CreateCurrencyCommand, Currency>()
            .ForMember(dest => dest.NormalizedName,
            opt => opt.MapFrom(src => src.Name.ToNormalized()));

        CreateMap<CurrencyCommand, Currency>()
            .ForMember(dest => dest.NormalizedName,
            opt => opt.MapFrom(src => src.Name.ToNormalized()));

        CreateMap<UpdateCurrencyCommand, Currency>()
            .ForMember(dest => dest.NormalizedName,
            opt => opt.MapFrom(src => src.Name.ToNormalized()));

        CreateMap<Currency, CurrencyDto>();
    }
}
