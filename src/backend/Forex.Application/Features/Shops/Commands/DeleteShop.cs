namespace Forex.Application.Features.Shops.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Shops;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record DeleteShopCommand(long Id) : IRequest<bool>;

public class DeleteShopCommandHandler(IAppDbContext context)
    : IRequestHandler<DeleteShopCommand, bool>
{
    public async Task<bool> Handle(DeleteShopCommand request, CancellationToken cancellationToken)
    {
        var shop = await context.Shops
            .Include(s => s.ShopCashes)
                .ThenInclude(a => a.Currency)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Shop), nameof(request.Id), request.Id);

        var nonZeroAccounts = shop.ShopCashes
            .Where(a => Math.Round(a.Balance, 2) != 0m || Math.Round(a.Discount, 2) != 0m)
            .ToList();

        if (nonZeroAccounts.Count != 0)
        {
            var details = nonZeroAccounts
                .Select(a => $"{a.Currency.Name}: Balance = {a.Balance}, Discount = {a.Discount}")
                .ToList();

            throw new ForbiddenException($"Kassada mablag' mavjud:\n{string.Join("\n", details)}");
        }

        shop.IsDeleted = true;
        foreach (var account in shop.ShopCashes)
            account.IsDeleted = true;

        return await context.SaveAsync(cancellationToken);
    }
}
