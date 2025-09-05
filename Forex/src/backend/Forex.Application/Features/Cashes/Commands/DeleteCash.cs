namespace Forex.Application.Features.Cashes.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

public record DeleteCashCommand(long Id) : IRequest<bool>;

public class DeleteCashCommandHandler(
    IAppDbContext context)
    : IRequestHandler<DeleteCashCommand, bool>
{
    public async Task<bool> Handle(DeleteCashCommand request, CancellationToken cancellationToken)
    {
        var cash = await context.Cashes
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Cash), nameof(request.Id), request.Id);

        if (!IsEmptyAccount(cash))
            throw new ForbiddenException($"Hisob: {cash.Balance}");

        cash.IsDeleted = true;
        return await context.SaveAsync(cancellationToken);
    }

    private static bool IsEmptyAccount(Cash cash)
        => Math.Round(cash.Balance, 2) == 0m;
}
