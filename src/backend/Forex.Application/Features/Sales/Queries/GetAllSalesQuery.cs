namespace Forex.Application.Features.Sales.Queries;

using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Sales.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllSalesQuery : IRequest<List<SaleListDto>>;

public class GetAllSalesQueryHandler(
    IAppDbContext context) : IRequestHandler<GetAllSalesQuery, List<SaleListDto>>
{
    public async Task<List<SaleListDto>> Handle(GetAllSalesQuery request, CancellationToken cancellationToken)
    {
        var sales = await context.Sales
            .Include(s => s.User)
            .AsNoTracking()
            .OrderByDescending(s => s.Date)
            .ToListAsync(cancellationToken);

        var dtoList = sales.Select(s => new SaleListDto
        {
            Id = s.Id,
            Date = s.Date,
            UserName = s.User.Name,
            TotalCount = s.TotalCount,
            TotalAmount = s.TotalAmount,
            CostPrice = s.CostPrice,
            BenifitPrice = s.BenifitPrice,
            Note = s.Note
        }).ToList();

        return dtoList;
    }
}
