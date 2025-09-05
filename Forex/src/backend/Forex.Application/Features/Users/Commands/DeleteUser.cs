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

public class DeleteUserCommandHandler(
    IAppDbContext context)
    : IRequestHandler<DeleteUserCommand, bool>
{
    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.Account)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), nameof(request.Id), request.Id);

        if (!IsEmptyAccount(user.Account))
            throw new ForbiddenException($"Hisob: {user.Account.Balance}, Chegirmasi: {user.Account.Discount}");

        user.IsDeleted = true;
        user.Account.IsDeleted = true;
        return await context.SaveAsync(cancellationToken);
    }

    private static bool IsEmptyAccount(Account account)
        => Math.Round(account.Discount, 2) == 0m &&
        Math.Round(account.Balance, 2) == 0m;
}
