namespace Forex.Application.Features.Products.ProductTypeItems.Queries;

using AutoMapper;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.Products.ProductTypeItems.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public record GetAllProductTypeItemQuery() : IRequest<IReadOnlyCollection<ProductTypeItemDto>>;

public class GetAllProductTypeItemHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllProductTypeItemQuery, IReadOnlyCollection<ProductTypeItemDto>>
{
    public async Task<IReadOnlyCollection<ProductTypeItemDto>> Handle(GetAllProductTypeItemQuery request, CancellationToken cancellationToken)
  => mapper.Map<IReadOnlyCollection<ProductTypeItemDto>>(await context.ProductTypeItems.AsNoTracking()
      .Include(a => a.SemiProduct)
        .ThenInclude(sp => sp.UnitMeasure)
      .ToListAsync(cancellationToken));
}