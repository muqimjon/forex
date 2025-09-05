namespace Forex.Application.Features.SemiProducts.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record UpdateSemiProductCommand(
    long Id,
    long ManufactoryId,
    string? Name,
    int Code,
    string Measure,
    string? PhotoPath)
    : IRequest<bool>;

public class UpdateSemiProductCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<UpdateSemiProductCommand, bool>
{
    public async Task<bool> Handle(UpdateSemiProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.SemiProducts
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(SemiProduct), nameof(request.Id), request.Id);

        mapper.Map(request, product);
        return await context.SaveAsync(cancellationToken);
    }
}
