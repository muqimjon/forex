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
    string? Username,
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

        // 5. Valyuta va Account mantiqini ishlash
        await AssignCurrencyByRole(user, request.Role, cancellationToken);

        context.Users.Add(user);
        await context.SaveAsync(cancellationToken);

        return user.Id;
    }

    private async Task AssignCurrencyByRole(User user, UserRole role, CancellationToken ct)
    {
        Currency? currency;

        if (role == UserRole.Vositachi)
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

        // Agar foydalanuvchining hisob raqamlari (Accounts) bo'lsa, birinchisiga valyutani bog'laymiz
        if (user.Accounts.Count != 0)
        {
            user.Accounts.First().Currency = currency;
        }
        else
        {
            // Hisob raqami bo'lmasa, yangi hisob ochamiz
            user.Accounts.Add(new UserAccount { Currency = currency });
        }
    }
}