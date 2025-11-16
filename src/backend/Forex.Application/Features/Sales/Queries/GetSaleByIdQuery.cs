namespace Forex.Application.Features.Sales.Queries;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Sales.DTOs;
using Forex.Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetSaleByIdQuery(long Id) : IRequest<SaleDto>;

public class GetSaleByIdQueryHandler(
    IAppDbContext context,
    IMapper mapper) : IRequestHandler<GetSaleByIdQuery, SaleDto>
{
    public async Task<SaleDto> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var sale = await context.Sales
            .Include(s => s.SaleItems)
                .ThenInclude(i => i.ProductType)
            .Include(s => s.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Sale), nameof(request.Id), request.Id);

        return mapper.Map<SaleDto>(sale);
    }
}
