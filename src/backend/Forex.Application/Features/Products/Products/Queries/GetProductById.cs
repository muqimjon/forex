namespace Forex.Application.Features.Products.Products.Queries;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Products.Products.DTOs;
using Forex.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetProductByIdQuery(long Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        => mapper.Map<ProductDto>(await context.Products
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken))
        ?? throw new NotFoundException(nameof(Product), nameof(request.Id), request.Id);
}
