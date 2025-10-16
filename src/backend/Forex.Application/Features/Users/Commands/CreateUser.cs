namespace Forex.Application.Features.Users.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Accounts.DTOs;
using Forex.Domain.Entities;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record CreateUserCommand(
    string Name,
    string? Phone,
    string? Email,
    UserRole Role,
    string? Address,
    string? Description,
    string? Password,
    List<CreateUserAccountCommand> Accounts)
    : IRequest<long>;

public class CreateUserCommandHandler(
    IAppDbContext context,
    IMapper mapper) : IRequestHandler<CreateUserCommand, long>
{
    public async Task<long> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var isExist = await context.Users
            .AnyAsync(user => user.Name == request.Name.Trim(), cancellationToken);

        if (isExist)
            throw new AlreadyExistException(nameof(User), nameof(request.Name), request.Name);

        var user = mapper.Map<User>(request);
        context.Users.Add(user);

        var currencies = await context.Currencies.ToListAsync(cancellationToken);
        var currencyIds = currencies.Select(c => c.Id).ToHashSet();

        foreach (var dto in request.Accounts)
        {
            if (!currencyIds.Contains(dto.CurrencyId))
                throw new NotFoundException(nameof(Currency), nameof(dto.CurrencyId), dto.CurrencyId);

            context.UserAccounts.Add(new UserAccount
            {
                User = user,
                CurrencyId = dto.CurrencyId,
                OpeningBalance = dto.OpeningBalance,
                Balance = dto.OpeningBalance,
                Discount = dto.Discount
            });
        }

        await context.SaveAsync(cancellationToken);
        return user.Id;
    }
}
