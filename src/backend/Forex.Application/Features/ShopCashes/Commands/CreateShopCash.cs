namespace Forex.Application.Features.ShopCashes.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Shops;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record CreateShopCashCommand(
    int ShopId,
    int CurrencyId,
    decimal Balance)
    : IRequest<long>;

public class CreateShopCashCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<CreateShopCashCommand, long>
{
    public async Task<long> Handle(CreateShopCashCommand request, CancellationToken cancellationToken)
    {
        var isExist = await context.ShopCashes
            .AnyAsync(c => c.ShopId == request.ShopId &&
                        c.CurrencyId == request.CurrencyId, cancellationToken);

        if (isExist)
            throw new AlreadyExistException(nameof(Shop), nameof(request.CurrencyId), request.CurrencyId);

        var cash = mapper.Map<ShopCash>(request);
        context.ShopCashes.Add(cash);

        await context.SaveAsync(cancellationToken);
        return cash.Id;
    }
}
