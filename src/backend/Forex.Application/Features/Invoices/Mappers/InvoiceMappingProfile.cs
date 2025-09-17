namespace Forex.Application.Features.Invoices.Mappers;

using AutoMapper;
using Forex.Application.Features.Invoices.DTOs;
using Forex.Domain.Entities.Manufactories;

public class InvoiceMappingProfile : Profile
{
    public InvoiceMappingProfile()
    {
        CreateMap<Invoice, InvoiceDto>();
    }
}
