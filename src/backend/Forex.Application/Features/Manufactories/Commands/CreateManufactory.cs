namespace Forex.Application.Features.Manufactories.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record CreateManufactoryCommand(
    string Name)
    : IRequest<long>;

public class CreateManufactoryCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<CreateManufactoryCommand, long>
{
    public async Task<long> Handle(CreateManufactoryCommand request, CancellationToken cancellationToken)
    {
        var isExist = await context.Manufactories
            .AnyAsync(c => c.Name == request.Name, cancellationToken);

        if (isExist)
            throw new AlreadyExistException(nameof(Shop), nameof(request.Name), request.Name);

        var Manufactory = mapper.Map<Manufactory>(request);
        context.Manufactories.Add(Manufactory);

        await context.SaveAsync(cancellationToken);
        return Manufactory.Id;
    }
}
