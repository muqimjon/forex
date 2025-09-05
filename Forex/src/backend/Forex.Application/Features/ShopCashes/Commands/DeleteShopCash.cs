namespace Forex.Application.Features.ShopCashes.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

public record DeleteShopCashCommand(long Id) : IRequest<bool>;

public class DeleteShopCashCommandHandler(
    IAppDbContext context)
    : IRequestHandler<DeleteShopCashCommand, bool>
{
    public async Task<bool> Handle(DeleteShopCashCommand request, CancellationToken cancellationToken)
    {
        var cash = await context.ShopCashes
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ShopCash), nameof(request.Id), request.Id);

        if (!IsEmptyAccount(cash))
            throw new ForbiddenException($"Hisob: {cash.Balance}");

        cash.IsDeleted = true;
        return await context.SaveAsync(cancellationToken);
    }

    private static bool IsEmptyAccount(ShopCash cash)
        => Math.Round(cash.Balance, 2) == 0m;
}
