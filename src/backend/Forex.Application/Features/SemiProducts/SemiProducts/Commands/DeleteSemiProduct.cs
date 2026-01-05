namespace Forex.Application.Features.SemiProducts.SemiProducts.Commands;

using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Interfaces;
using Forex.Domain.Entities.SemiProducts;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
