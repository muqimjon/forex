namespace Forex.Application.Features.Sales.Mappers;

using AutoMapper;
using Forex.Application.Features.Sales.Commands;
using Forex.Application.Features.Sales.SaleItems.DTOs;
using Forex.Domain.Entities.Sales;

public class SaleMappingProfile : Profile
{
    public SaleMappingProfile()
    {
        CreateMap<CreateSaleCommand, Sale>();
        CreateMap<SaleItemCreateDto, SaleItem>();
    }
}
