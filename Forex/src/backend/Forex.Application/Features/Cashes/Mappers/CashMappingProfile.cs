namespace Forex.Application.Features.Cashes.Mappers;

using AutoMapper;
using Forex.Application.Features.Cashes.Commands;
using Forex.Application.Features.Cashes.DTOs;
using Forex.Domain.Entities;

public class CashMappingProfile : Profile
{
    public CashMappingProfile()
    {
        CreateMap<CreateCashCommand, Cash>();
        CreateMap<UpdateCashCommand, Cash>();
        CreateMap<Cash, CashDto>();
    }
}
