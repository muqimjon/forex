namespace Forex.Application.Features.SemiProducts.SemiProductEntries.Mappers;

using AutoMapper;
using Forex.Application.Features.SemiProducts.SemiProductEntries.DTOs;
using Forex.Domain.Entities.SemiProducts;

public class SemiProductMappingProfile : Profile
{
    public SemiProductMappingProfile()
    {
        CreateMap<SemiProductEntry, SemiProductEntryDto>();
        CreateMap<SemiProductEntry, SemiProductEntryForInvoiceDto>();
        CreateMap<SemiProductEntry, SemiProductEntryForManufactoryDto>();
        CreateMap<SemiProductEntry, SemiProductEntryForSemiProductDto>();
    }
}
