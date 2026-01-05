namespace Forex.Application.Features.Manufactories.Queries;

using AutoMapper;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.Manufactories.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GetAllManufactoriesQuery : IRequest<IReadOnlyCollection<ManufactoryDto>>;

public class GetAllManufactoriesQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllManufactoriesQuery, IReadOnlyCollection<ManufactoryDto>>
{
    public async Task<IReadOnlyCollection<ManufactoryDto>> Handle(GetAllManufactoriesQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<ManufactoryDto>>(await context.Manufactories
            .AsNoTracking()
            .ToListAsync(cancellationToken));
}
