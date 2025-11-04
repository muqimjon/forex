namespace Forex.Application.Features.UnitMeasures.Commands;

using AutoMapper;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateAllUnitMeasuresCommand(List<UnitMeasureCommand> Items) : IRequest<bool>;

public class UpdateAllUnitMeasuresCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<UpdateAllUnitMeasuresCommand, bool>
{
    public async Task<bool> Handle(UpdateAllUnitMeasuresCommand request, CancellationToken cancellationToken)
    {
        var incoming = request.Items;
        var existing = await context.UnitMeasures.ToListAsync(cancellationToken);

        var toDelete = existing
            .Where(db => incoming.All(dto => dto.Id != db.Id))
            .ToList();

        context.UnitMeasures.RemoveRange(toDelete);

        foreach (var dto in incoming)
        {
            var entity = existing.FirstOrDefault(e => e.Id == dto.Id);

            if (entity is null)
            {
                var newEntity = mapper.Map<UnitMeasure>(dto);
                await context.UnitMeasures.AddAsync(newEntity, cancellationToken);
            }
            else
                mapper.Map(dto, entity);
        }

        return await context.SaveAsync(cancellationToken);
    }
}
