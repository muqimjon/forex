namespace Forex.Application.Features.Currencies.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record DeleteCurrencyCommand(long Id) : IRequest<bool>;

public class DeleteCurrencyCommandHandler(
    IAppDbContext context)
    : IRequestHandler<DeleteCurrencyCommand, bool>
{
    public async Task<bool> Handle(DeleteCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await context.Currencies
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Currency), nameof(request.Id), request.Id);

        currency.IsDeleted = true;
        return await context.SaveAsync(cancellationToken);
    }
}
