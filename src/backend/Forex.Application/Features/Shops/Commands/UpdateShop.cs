namespace Forex.Application.Features.Shops.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Shops;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateShopCommand(
    long Id,
    string Name)
    : IRequest<bool>;

public class UpdateShopCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<UpdateShopCommand, bool>
{
    public async Task<bool> Handle(UpdateShopCommand request, CancellationToken cancellationToken)
    {
        var shop = await context.Shops
            .Include(s => s.ShopCashes)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Shop), nameof(request.Id), request.Id);

        mapper.Map(request, shop);

        var currencies = await context.Currencies.ToListAsync(cancellationToken);
        var existingCurrencyIds = shop.ShopCashes.Select(a => a.CurrencyId).ToHashSet();

        foreach (var currency in currencies)
        {
            if (!existingCurrencyIds.Contains(currency.Id))
            {
                context.ShopCashAccounts.Add(new ShopCashAccount
                {
                    Shop = shop,
                    CurrencyId = currency.Id,
                    OpeningBalance = 0m,
                    Balance = 0m,
                    Discount = 0m
                });
            }
        }

        return await context.SaveAsync(cancellationToken);
    }
}
