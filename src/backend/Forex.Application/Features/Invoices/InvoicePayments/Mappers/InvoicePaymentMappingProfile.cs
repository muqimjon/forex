namespace Forex.Application.Features.Invoices.InvoicePayments.Mappers;

using Forex.Application.Features.Invoices.InvoicePayments.Commands;
using Forex.Application.Features.Invoices.InvoicePayments.DTOs;
using Forex.Application.Features.Sales.SaleItems.Mappers;
using Forex.Domain.Entities;

public class InvoicePaymentMappingProfile : MappingProfile
{
    public InvoicePaymentMappingProfile()
    {
        CreateMap<InvoicePayment, InvoicePaymentDto>();
        CreateMap<InvoicePaymentCommand, InvoicePayment>();
    }
}
