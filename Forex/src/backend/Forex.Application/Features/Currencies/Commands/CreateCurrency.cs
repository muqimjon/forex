namespace Forex.Application.Features.Currencies.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record CreateCurrencyCommand(
    string Name,
    string Symbol)
    : IRequest<long>;

public class CreateCurrencyCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<CreateCurrencyCommand, long>
{
    public async Task<long> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var isExist = await context.Currencies
            .AnyAsync(c => c.Name == request.Name &&
                        c.Symbol == request.Symbol, cancellationToken);

        if (isExist)
            throw new AlreadyExistException(nameof(Shop), nameof(request.Name), request.Name);

        var Currency = mapper.Map<Currency>(request);
        context.Currencies.Add(Currency);

        await context.SaveAsync(cancellationToken);
        return Currency.Id;
    }
}
