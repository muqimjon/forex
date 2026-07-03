namespace Forex.Application.Features.Users.Commands;

using AutoMapper;
using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Extensions;
using Forex.Application.Common.Interfaces;
using Forex.Application.Features.Accounts.Commands;
using Forex.Domain.Entities;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record CreateUserCommand(
    string Name,
    string? Username,
    string? Phone,
    string? Email,
    UserRole Role,
    string? Address,
    string? Description,
    string? Password,
    long AccessMask,
    List<CreateUserAccountCommand> Accounts)
    : IRequest<long>;

public class CreateUserCommandHandler(
    IAppDbContext context,
    IMapper mapper,
    IPasswordHasher hasher) : IRequestHandler<CreateUserCommand, long>
{
    public async Task<long> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Ism bo'yicha takrorlanishni tekshirish (Normalized)
        var isNameExist = await context.Users
            .AnyAsync(u => u.NormalizedName == request.Name.ToNormalized(), cancellationToken);

        if (isNameExist)
            throw new AlreadyExistException(nameof(User), nameof(request.Name), request.Name);

        // 2. Login (Username) bandligini tekshirish (agar yuborilgan bo'lsa)
        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            var isUsernameExist = await context.Users
                .AnyAsync(u => u.Username == request.Username, cancellationToken);

            if (isUsernameExist)
                throw new AlreadyExistException("Ushbu login allaqachon band.");
        }

        // 3. Mapping
        var user = mapper.Map<User>(request);

        // 4. Parol bo'lsa, hashlash
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = hasher.HashPassword(request.Password);
        }

        // 5. Asosiy valyuta va hisob
        await AssignSettlementCurrency(user, request.Role, cancellationToken);

        context.Users.Add(user);
        await context.SaveAsync(cancellationToken);

        return user.Id;
    }

    private async Task AssignSettlementCurrency(User user, UserRole role, CancellationToken ct)
    {
        var account = user.Accounts.FirstOrDefault();

        Currency? currency = account is not null && account.CurrencyId > 0
            ? await context.Currencies.FirstOrDefaultAsync(c => c.Id == account.CurrencyId, ct)
            : null;

        currency ??= await ResolveCurrencyByRole(role, ct);

        user.SettlementCurrency = currency;

        if (account is null)
        {
            account = new UserAccount();
            user.Accounts.Add(account);
        }

        account.Currency = currency;
        if (currency.Id != 0)
            account.CurrencyId = currency.Id;
        account.Balance = account.OpeningBalance;
    }

    private async Task<Currency> ResolveCurrencyByRole(UserRole role, CancellationToken ct)
    {
        Currency? currency;

        if (role == UserRole.Vositachi || role == UserRole.Taminotchi)
        {
            currency = await context.Currencies
                .FirstOrDefaultAsync(c => c.NormalizedName == "Dollar".ToNormalized(), ct);

            currency ??= new Currency
            {
                Name = "Dollar",
                NormalizedName = "Dollar".ToNormalized(),
                Code = "USD",
                Symbol = "$",
                IsEditable = true,
                IsActive = true
            };
        }
        else if (role == UserRole.Mijoz)
        {
            currency = await context.Currencies
                .FirstOrDefaultAsync(c => c.NormalizedName == "So'm".ToNormalized(), ct);

            currency ??= new Currency
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
        }
        else
        {
            // Boshqa barcha rollar (Hodim, Taminotchi, User) uchun default valyuta
            currency = await context.Currencies.FirstOrDefaultAsync(c => c.IsDefault, ct)
                ?? throw new AppException("Tizimda standart valyuta (So'm) topilmadi!");

            if (currency.NormalizedName != "So'm".ToNormalized())
                throw new AppException("So'm standart valyuta sifatida tanlanishi shart!");
        }

        return currency;
    }
}