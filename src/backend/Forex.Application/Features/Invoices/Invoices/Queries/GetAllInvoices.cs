namespace Forex.Application.Features.Invoices.Invoices.Queries;

using AutoMapper;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.Invoices.Invoices.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllInvoicesQuery : IRequest<List<InvoiceDto>>;

public class GetAllInvoicesQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllInvoicesQuery, List<InvoiceDto>>
{
    public async Task<List<InvoiceDto>> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
    => mapper.Map<List<InvoiceDto>>(await context.Invoices
        .Include(i => i.SemiProductEntries)
        .AsQueryable()
        .ToListAsync(cancellationToken));
}
