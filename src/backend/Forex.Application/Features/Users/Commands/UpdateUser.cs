namespace Forex.Application.Features.Users.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using Forex.Domain.Entities.Users;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

public record UpdateUserCommand(
    long Id,
    string Name,
    string Phone,
    string? Email,
    Role Role,
    string Address,
    string Description,
    decimal Balance,
    long CurrencyId)
    : IRequest<bool>;


public class UpdateUserCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<UpdateUserCommand, bool>
{
    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), nameof(request.Id), request.Id);

        mapper.Map(request, user);

        var account = user.Accounts.FirstOrDefault(a => a.CurrencyId == request.CurrencyId);

        if (account is null)
        {
            account = new UserAccount
            {
                User = user,
                CurrencyId = request.CurrencyId,
                OpeningBalance = request.Balance,
                Balance = request.Balance
            };

            context.UserAccounts.Add(account);
        }
        else
        {
            account.OpeningBalance = request.Balance;
            account.Balance = request.Balance;
        }

        return await context.SaveAsync(cancellationToken);
    }
}
