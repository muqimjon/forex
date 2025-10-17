namespace Forex.Application.Features.Invoices.Mappers;

using AutoMapper;
using Forex.Application.Commons.Extensions;
using Forex.Application.Features.Invoices.Commands;
using Forex.Application.Features.Invoices.DTOs;
using Forex.Domain.Entities;

public class InvoiceMappingProfile : Profile
{
    public InvoiceMappingProfile()
    {
        CreateMap<Invoice, InvoiceDto>();
        CreateMap<Invoice, InvoiceForManufactoryDto>();
        CreateMap<Invoice, InvoiceForSemiProductEntryDto>();
        CreateMap<Invoice, InvoiceForUserDto>();

        CreateMap<InvoiceCommand, Invoice>()
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToUtcSafe()));
    }
}
