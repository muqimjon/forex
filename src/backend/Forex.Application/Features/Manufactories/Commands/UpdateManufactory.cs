namespace Forex.Application.Features.Manufactories.Commands;

using AutoMapper;
using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateManufactoryCommand(
    long Id,
    string Name)
    : IRequest<bool>;
public class UpdateManufactoryCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<UpdateManufactoryCommand, bool>
{
    public async Task<bool> Handle(UpdateManufactoryCommand request, CancellationToken cancellationToken)
    {
        var manufactory = await context.Manufactories
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Manufactory), nameof(request.Id), request.Id);

        mapper.Map(request, manufactory);
        return await context.SaveAsync(cancellationToken);
    }
}
