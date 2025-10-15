namespace Forex.Application.Features.UnitMeasures.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateUnitMeasureCommand(
    long Id,
    string Name,
    string Symbol,
    bool IsDefault,
    bool IsActive,
    string Description)
    : IRequest<bool>;
public class UpdateUnitMeasureCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<UpdateUnitMeasureCommand, bool>
{
    public async Task<bool> Handle(UpdateUnitMeasureCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.UnitMeasures
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(UnitMeasure), nameof(request.Id), request.Id);

        mapper.Map(request, entity);
        return await context.SaveAsync(cancellationToken);
    }
}
