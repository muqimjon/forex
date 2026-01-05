namespace Forex.Application.Features.Products.ProductTypes.Queries;

using AutoMapper;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.Products.ProductTypes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public record GetAllProductTypesQuery() : IRequest<IReadOnlyCollection<ProductTypeDto>>;

public class GetAllProductTypesHandler(
    IAppDbContext context,
    IMapper mapper) : IRequestHandler<GetAllProductTypesQuery, IReadOnlyCollection<ProductTypeDto>>
{
    public async Task<IReadOnlyCollection<ProductTypeDto>> Handle(GetAllProductTypesQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<ProductTypeDto>>(await context.ProductTypes.AsNoTracking().ToListAsync(cancellationToken));
}