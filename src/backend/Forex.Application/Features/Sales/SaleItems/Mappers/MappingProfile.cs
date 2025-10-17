namespace Forex.Application.Features.Sales.SaleItems.Mappers;

using AutoMapper;
using Forex.Application.Features.Sales.SaleItems.Commands;
using Forex.Application.Features.Sales.SaleItems.DTOs;
using Forex.Domain.Entities.Sales;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<SaleItemCommand, SaleItem>();

        CreateMap<SaleItem, SaleItemDto>();
        CreateMap<SaleItem, SaleItemForProductTypeDto>();
        CreateMap<SaleItem, SaleItemForSaleDto>();
    }
}
