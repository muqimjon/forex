namespace Forex.Application.Features.Users.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Accounts.DTOs;
using Forex.Domain.Entities;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateUserCommand(
    long Id,
    string Name,
    string? Phone,
    string? Email,
    UserRole Role,
    string? Address,
    string? Description,
    string? Password,
    List<UpdateUserAccountCommand> Accounts)
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

        var currencies = await context.Currencies.ToListAsync(cancellationToken);
        var currencyIds = currencies.Select(c => c.Id).ToHashSet();

        foreach (var dto in request.Accounts)
        {
            if (!currencyIds.Contains(dto.CurrencyId))
                throw new NotFoundException(nameof(Currency), nameof(dto.CurrencyId), dto.CurrencyId);

            var account = user.Accounts.FirstOrDefault(a => a.CurrencyId == dto.CurrencyId);

            if (account is null)
            {
                account = new UserAccount
                {
                    User = user,
                    CurrencyId = dto.CurrencyId,
                    OpeningBalance = dto.Balance,
                    Balance = dto.Balance,
                    Discount = dto.Discount
                };

                context.UserAccounts.Add(account);
            }
            else
            {
                account.OpeningBalance = dto.Balance;
                account.Balance = dto.Balance;
                account.Discount = dto.Discount;
            }
        }

        return await context.SaveAsync(cancellationToken);
    }
}
