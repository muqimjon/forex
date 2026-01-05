namespace Forex.Application.Features.SemiProducts.SemiProducts.Queries;

using AutoMapper;
using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.SemiProducts.SemiProducts.DTOs;
using Forex.Domain.Entities.SemiProducts;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetSemiProductByIdQuery(long Id) : IRequest<SemiProductDto>;

public class GetSemiProductByIdQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetSemiProductByIdQuery, SemiProductDto>
{
    public async Task<SemiProductDto> Handle(GetSemiProductByIdQuery request, CancellationToken cancellationToken)
        => mapper.Map<SemiProductDto>(await context.SemiProducts
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken))
        ?? throw new NotFoundException(nameof(SemiProduct), nameof(request.Id), request.Id);
}
