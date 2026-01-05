namespace Forex.Application.Features.Invoices.Invoices.Queries;

using AutoMapper;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Common.Models;
using Forex.Application.Features.Invoices.Invoices.DTOs;
using MediatR;

public record InvoiceFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<InvoiceDto>>;

public class InvoiceFilterQueryHandler(
    IAppDbContext context,
    IMapper mapper,
    IPagingMetadataWriter writer)
    : IRequestHandler<InvoiceFilterQuery, IReadOnlyCollection<InvoiceDto>>
{
    public async Task<IReadOnlyCollection<InvoiceDto>> Handle(InvoiceFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<InvoiceDto>>(await context.Invoices
            .ToPagedListAsync(request, writer, cancellationToken));
}