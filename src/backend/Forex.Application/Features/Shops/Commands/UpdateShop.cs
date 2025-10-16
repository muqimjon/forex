namespace Forex.Application.Features.Shops.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Accounts.Commands;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateShopCommand(
    long Id,
    string Name,
    List<UpdateShopAccountCommand> AccountsCommand)
    : IRequest<bool>;

public class UpdateShopCommandHandler(
    IAppDbContext context,
    IMapper mapper
) : IRequestHandler<UpdateShopCommand, bool>
{
    public async Task<bool> Handle(UpdateShopCommand request, CancellationToken cancellationToken)
    {
        var shop = await context.Shops
            .Include(s => s.ShopAccounts)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Shop), nameof(request.Id), request.Id);

        mapper.Map(request, shop);

        foreach (var accountCmd in request.AccountsCommand)
        {
            var existing = shop.ShopAccounts.FirstOrDefault(x => x.Id == accountCmd.Id);

            if (existing is not null)
                mapper.Map(accountCmd, existing);
            else
                shop.ShopAccounts.Add(mapper.Map<ShopAccount>(accountCmd));
        }

        return await context.SaveAsync(cancellationToken);
    }
}
