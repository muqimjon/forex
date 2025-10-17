namespace Forex.Application.Features.Manufactories.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteManufactoryCommand(long Id) : IRequest<bool>;

public class DeleteManufactoryCommandHandler(
    IAppDbContext context)
    : IRequestHandler<DeleteManufactoryCommand, bool>
{
    public async Task<bool> Handle(DeleteManufactoryCommand request, CancellationToken cancellationToken)
    {
        var manufactory = await context.Manufactories
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Manufactory), nameof(request.Id), request.Id);

        manufactory.IsDeleted = true;
        return await context.SaveAsync(cancellationToken);
    }
}
