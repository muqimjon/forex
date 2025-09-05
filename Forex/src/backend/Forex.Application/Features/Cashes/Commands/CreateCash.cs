namespace Forex.Application.Features.Cashes.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record CreateCashCommand(
    int ShopId,
    int CurrencyId,
    decimal Balance)
    : IRequest<long>;

public class CreateCashCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<CreateCashCommand, long>
{
    public async Task<long> Handle(CreateCashCommand request, CancellationToken cancellationToken)
    {
        var isExist = await context.Cashes
            .AnyAsync(c => c.ShopId == request.ShopId &&
                        c.CurrencyId == request.CurrencyId, cancellationToken);

        if (isExist)
            throw new AlreadyExistException(nameof(Shop), nameof(request.CurrencyId), request.CurrencyId);

        var cash = mapper.Map<Cash>(request);
        context.Cashes.Add(cash);

        await context.SaveAsync(cancellationToken);
        return cash.Id;
    }
}
