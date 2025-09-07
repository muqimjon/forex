namespace Forex.Application.Features.SemiProducts.Queries;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.SemiProducts.DTOs;
using Forex.Domain.Entities.Manufactories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

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
