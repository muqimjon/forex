namespace Forex.Application.Features.Invoices.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Invoices.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record GetAllInvoicesQuery : IRequest<List<InvoiceDto>>;

public class GetAllInvoicesQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllInvoicesQuery, List<InvoiceDto>>
{
    public async Task<List<InvoiceDto>> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
    => mapper.Map<List<InvoiceDto>>(await context.Invoices
        .Include(i => i.ContainerEntries)
            .ThenInclude(ce => ce.Sender)
        .Include(i => i.SemiProducts)
        .Include(i => i.SemiProducts)
        .AsQueryable()
        .ToListAsync(cancellationToken));
}
