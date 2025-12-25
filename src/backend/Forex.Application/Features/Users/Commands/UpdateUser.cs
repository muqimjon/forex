namespace Forex.Application.Features.Users.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Extensions; // .ToNormalized() uchun
using Forex.Application.Commons.Interfaces;
using Forex.Domain.Entities;
using Forex.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UpdateUserAccountDto
{
    public long CurrencyId { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
}

public record UpdateUserCommand(
    long Id,
    string Name,
    string? Username,
    string? Phone,
    string? Email,
    UserRole Role,
    string? Address,
    string? Description,
    string? Password,
    List<UpdateUserAccountDto> Accounts) : IRequest<bool>;

public class UpdateUserCommandHandler(
    IAppDbContext context,
    IPasswordHasher hasher,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateUserCommand, bool>
{
    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Userni bazadan topish
        var user = await context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User));

        // 2. 🔥 XAVFSIZLIK TEKSHIRUVI
        bool isAdmin = currentUser.Username == "admin";
        bool isSelf = currentUser.UserId == request.Id;

        if (!string.IsNullOrWhiteSpace(request.Username) && !isAdmin)
            throw new AppException($"Sizda {request.Username}ni o'zgartirish huquqi yo'q!");

        // 3. Ism o'zgarayotgan bo'lsa, bazada boshqa odamda yo'qligini tekshirish
        var normalizedNewName = request.Name.ToNormalized();
        if (user.NormalizedName != normalizedNewName)
        {
            var nameExists = await context.Users.AnyAsync(u => u.NormalizedName == normalizedNewName && u.Id != user.Id, cancellationToken);
            if (nameExists) throw new AlreadyExistException("Ushbu ismli foydalanuvchi allaqachon mavjud.");

            user.Name = request.Name;
            user.NormalizedName = normalizedNewName;
        }

        // 4. Asosiy maydonlarni yangilash
        user.Phone = request.Phone;
        user.Email = request.Email; // Sening kodingda bu qolib ketgan edi
        user.Address = request.Address;
        user.Description = request.Description;

        if (isAdmin)
        {
            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                var loginExists = await context.Users.AnyAsync(u => u.Username == request.Username && u.Id != user.Id, cancellationToken);
                if (loginExists) throw new AppException("Bu login allaqachon band!");
            }
            user.Username = request.Username; // Bo'sh bo'lsa login o'chiriladi (access revoked)
        }

        // 6. Parol yangilash (agar kiritilgan bo'lsa)
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = hasher.HashPassword(request.Password);
        }

        // 7. Accountlarni yangilash
        //UpdateAccounts(user, request.Accounts);

        context.Users.Update(user);
        var result = await context.SaveAsync(cancellationToken);

        return result;
    }

    private void UpdateAccounts(User user, List<UpdateUserAccountDto> dtos)
    {
        if (dtos == null || dtos.Count == 0) return;

        foreach (var dto in dtos)
        {
            var existingAcc = user.Accounts.FirstOrDefault(a => a.CurrencyId == dto.CurrencyId);
            if (existingAcc != null)
            {
                existingAcc.OpeningBalance = dto.OpeningBalance;
                existingAcc.Balance = dto.OpeningBalance; // Eslatma: Bu mantiq eski balansni o'chirib yuboradi
                existingAcc.Discount = dto.Discount;
            }
            else
            {
                user.Accounts.Add(new UserAccount
                {
                    CurrencyId = dto.CurrencyId,
                    OpeningBalance = dto.OpeningBalance,
                    Balance = dto.OpeningBalance,
                    Discount = dto.Discount,
                    UserId = user.Id
                });
            }
        }
    }
}