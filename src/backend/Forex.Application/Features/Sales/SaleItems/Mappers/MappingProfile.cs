namespace Forex.Application.Features.Sales.SaleItems.Mappers;

using AutoMapper;
using Forex.Application.Features.Sales.SaleItems.DTOs;
using Forex.Domain.Entities.Sales;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<SaleItemCreateDto, SaleItem>();
    }
}
