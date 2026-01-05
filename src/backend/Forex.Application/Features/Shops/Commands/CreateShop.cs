namespace Forex.Application.Features.Shops.Commands;

using AutoMapper;
using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.Accounts.Commands;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record CreateShopCommand(
    string Name,
    List<CreateShopAccountCommand> Accounts)
    : IRequest<long>;

public class CreateShopCommandHandler(
    IAppDbContext context,
    IMapper mapper) : IRequestHandler<CreateShopCommand, long>
{
    public async Task<long> Handle(CreateShopCommand request, CancellationToken cancellationToken)
    {
        var isExist = await context.Shops
            .AnyAsync(shop => shop.Name == request.Name, cancellationToken);

        if (isExist)
            throw new AlreadyExistException(nameof(Shop), nameof(request.Name), request.Name);

        var shop = mapper.Map<Shop>(request);
        context.Shops.Add(shop);

        var currencies = await context.Currencies.ToListAsync(cancellationToken);
        foreach (var currency in currencies)
        {
            context.ShopCashAccounts.Add(new ShopAccount
            {
                Shop = shop,
                CurrencyId = currency.Id,
                OpeningBalance = 0m,
                Balance = 0m,
                Discount = 0m
            });
        }

        await context.SaveAsync(cancellationToken);
        return shop.Id;
    }
}
