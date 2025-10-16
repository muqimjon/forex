namespace Forex.Application.Features.Sales.Mappers;

using AutoMapper;
using Forex.Application.Features.Sales.Commands;
using Forex.Application.Features.Sales.DTOs;
using Forex.Domain.Entities.Sales;

public class SaleMappingProfile : Profile
{
    public SaleMappingProfile()
    {
        CreateMap<CreateSaleCommand, Sale>();
        CreateMap<UpdateSaleCommand, Sale>();

        CreateMap<Sale, SaleDto>();
        CreateMap<Sale, SaleForSaleItemDto>();
        CreateMap<Sale, SaleForUserDto>();
    }
}
