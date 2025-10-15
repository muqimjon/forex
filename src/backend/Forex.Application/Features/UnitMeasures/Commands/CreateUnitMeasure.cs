namespace Forex.Application.Features.UnitMeasures.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record CreateUnitMeasureCommand(
    string Name,
    string Symbol,
    bool IsDefault,
    bool IsActive,
    string Description)
    : IRequest<long>;

public class CreateUnitMeasureCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<CreateUnitMeasureCommand, long>
{
    public async Task<long> Handle(CreateUnitMeasureCommand request, CancellationToken cancellationToken)
    {
        var isExist = await context.UnitMeasures
            .AnyAsync(c => c.Name == request.Name, cancellationToken);

        if (isExist)
            throw new AlreadyExistException(nameof(UnitMeasure), nameof(request.Name), request.Name);

        var entity = mapper.Map<UnitMeasure>(request);
        context.UnitMeasures.Add(entity);

        await context.SaveAsync(cancellationToken);
        return entity.Id;
    }
}
