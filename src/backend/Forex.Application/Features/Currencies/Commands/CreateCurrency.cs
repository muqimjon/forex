namespace Forex.Application.Features.Currencies.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record CreateCurrencyCommand(
    string Name,
    string Symbol,
    bool IsDefault)
    : IRequest<long>;


public class CreateCurrencyCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<CreateCurrencyCommand, long>
{
    public async Task<long> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var isExist = await context.Currencies
            .AnyAsync(c => c.Name == request.Name && c.Symbol == request.Symbol, cancellationToken);

        if (isExist)
            throw new AlreadyExistException(nameof(Currency), nameof(request.Name), request.Name);

        var currency = mapper.Map<Currency>(request);
        context.Currencies.Add(currency);

        if (request.IsDefault)
        {
            var others = await context.Currencies
                .Where(c => c.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var other in others)
                other.IsDefault = false;
        }

        await context.SaveAsync(cancellationToken);
        return currency.Id;
    }
}
