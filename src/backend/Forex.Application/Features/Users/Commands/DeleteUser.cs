namespace Forex.Application.Features.Users.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

public record DeleteUserCommand(long Id) : IRequest<bool>;

public class DeleteUserCommandHandler(IAppDbContext context)
    : IRequestHandler<DeleteUserCommand, bool>
{
    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.Accounts)
                .ThenInclude(a => a.Currency)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), nameof(request.Id), request.Id);

        var nonZeroAccounts = user.Accounts
            .Where(a => Math.Round(a.Balance, 2) != 0m || Math.Round(a.Discount, 2) != 0m)
            .ToList();

        if (nonZeroAccounts.Count != 0)
        {
            var details = nonZeroAccounts
                .Select(a => $"{a.Currency.Name}: Balance = {a.Balance}, Discount = {a.Discount}")
                .ToList();

            throw new ForbiddenException($"Foydalanuvchi hisobida mablag' mavjud:\n{string.Join("\n", details)}");
        }

        user.IsDeleted = true;
        foreach (var account in user.Accounts)
            account.IsDeleted = true;

        return await context.SaveAsync(cancellationToken);
    }
}
