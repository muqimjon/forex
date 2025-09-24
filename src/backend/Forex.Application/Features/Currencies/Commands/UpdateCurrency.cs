namespace Forex.Application.Features.Currencies.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record UpdateCurrencyCommand(
    long Id,
    string Name,
    string Symbol,
    bool IsDefault)
    : IRequest<bool>;

public class UpdateCurrencyCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<UpdateCurrencyCommand, bool>
{
    public async Task<bool> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await context.Currencies
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Currency), nameof(request.Id), request.Id);

        mapper.Map(request, currency);

        if (request.IsDefault)
        {
            var others = await context.Currencies
                .Where(c => c.Id != request.Id && c.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var other in others)
                other.IsDefault = false;
        }

        return await context.SaveAsync(cancellationToken);
    }
}
