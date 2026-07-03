namespace Forex.Application.Features.Products.ProductTypes.Queries;

using AutoMapper;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.Products.ProductTypes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetProductTypeByBarcodeQuery(string Code) : IRequest<ProductTypeDto?>;

public class GetProductTypeByBarcodeQueryHandler(IAppDbContext context, IMapper mapper)
    : IRequestHandler<GetProductTypeByBarcodeQuery, ProductTypeDto?>
{
    public async Task<ProductTypeDto?> Handle(GetProductTypeByBarcodeQuery request, CancellationToken ct)
    {
        var entity = await context.ProductTypes
            .AsNoTracking()
            .Include(t => t.Product)
            .Include(t => t.ProductResidue)
            .FirstOrDefaultAsync(t => t.QopBarcode == request.Code || t.PackBarcode == request.Code, ct);

        return entity is null ? null : mapper.Map<ProductTypeDto>(entity);
    }
}
