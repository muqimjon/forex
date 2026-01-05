namespace Forex.Application.Features.UnitMeasures.Commands;

using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteUnitMeasureCommand(long Id) : IRequest<bool>;

public class DeleteUnitMeasureCommandHandler(
    IAppDbContext context)
    : IRequestHandler<DeleteUnitMeasureCommand, bool>
{
    public async Task<bool> Handle(DeleteUnitMeasureCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.UnitMeasures
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(UnitMeasure), nameof(request.Id), request.Id);

        entity.IsDeleted = true;
        return await context.SaveAsync(cancellationToken);
    }
}
