namespace Forex.Application.Features.Manufactories.Queries;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Manufactories.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetManufactoryByIdQuery(long Id) : IRequest<ManufactoryDto>;

public class GetManufactoryByIdQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetManufactoryByIdQuery, ManufactoryDto>
{
    public async Task<ManufactoryDto> Handle(GetManufactoryByIdQuery request, CancellationToken cancellationToken)
        => mapper.Map<ManufactoryDto>(await context.Manufactories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken));
}
