namespace Forex.Application.Features.SemiProducts.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record DeleteSemiProductCommand(long Id) : IRequest<bool>;

public class DeleteSemiProductCommandHandler(
    IAppDbContext context)
    : IRequestHandler<DeleteSemiProductCommand, bool>
{
    public async Task<bool> Handle(DeleteSemiProductCommand request, CancellationToken cancellationToken)
    {
        var semiProduct = await context.SemiProducts
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(SemiProduct), nameof(request.Id), request.Id);

        semiProduct.IsDeleted = true;
        return await context.SaveAsync(cancellationToken);
    }
}
