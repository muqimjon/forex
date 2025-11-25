namespace Forex.Application.Features.Users.Commands;

using AutoMapper;
using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Extensions;
using Forex.Application.Commons.Interfaces;
using Forex.Application.Features.Accounts.Commands;
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
            .AnyAsync(user => user.NormalizedName == request.Name.ToNormalized(), cancellationToken);

        if (isExist)
            throw new AlreadyExistException(nameof(User), nameof(request.Name), request.Name);

        var user = mapper.Map<User>(request);

        if (request.Role == UserRole.Vositachi)
        {
            var currency = await context.Currencies.FirstOrDefaultAsync(c => c.NormalizedName == "Dollar".ToNormalized(), cancellationToken);

            currency ??= new()
            {
                Name = "Dollar",
                NormalizedName = "Dollar".ToNormalized(),
                Code = "USD",
                Symbol = "$",
                IsEditable = true,
                IsActive = true
            };

            if (user.Accounts.Count != 0)
                user.Accounts.First().Currency = currency;
            else user.Accounts.Add(new() { Currency = currency });
        }
        else if (request.Role == UserRole.Mijoz)
        {
            var currency = await context.Currencies.FirstOrDefaultAsync(c => c.NormalizedName == "So'm".ToNormalized(), cancellationToken);

            currency ??= new()
            {
                Name = "So'm",
                NormalizedName = "So'm".ToNormalized(),
                Code = "UZS",
                Symbol = "so'm",
                IsEditable = false,
                IsActive = true,
                IsDefault = true,
                ExchangeRate = 1
            };

            if (user.Accounts.Count != 0)
                user.Accounts.First().Currency = currency;
            else user.Accounts.Add(new() { Currency = currency });
        }
        else
        {
            var currency = await context.Currencies.FirstOrDefaultAsync(c => c.IsDefault, cancellationToken)
                ?? throw new AppException("Standart valyuta kiritilmagan");

            if (currency.NormalizedName != "So'm".ToNormalized())
                throw new AppException("So'm Standart valyuta sifatida tanlanishi shart!");

            if (user.Accounts.Count != 0)
                user.Accounts.First().Currency = currency;
            else user.Accounts.Add(new() { Currency = currency });
        }

        context.Users.Add(user);

        await context.SaveAsync(cancellationToken);
        return user.Id;
    }
}