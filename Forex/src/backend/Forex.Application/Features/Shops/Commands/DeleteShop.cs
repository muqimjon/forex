namespace Forex.Application.Features.Shops.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record DeleteShopCommand(long Id) : IRequest<bool>;

public class DeleteShopCommandHandler(
    IAppDbContext context)
    : IRequestHandler<DeleteShopCommand, bool>
{
    public async Task<bool> Handle(DeleteShopCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Shops
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Shop), nameof(request.Id), request.Id);

        product.IsDeleted = true;
        return await context.SaveAsync(cancellationToken);
    }
}
