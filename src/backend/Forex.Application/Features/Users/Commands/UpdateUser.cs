namespace Forex.Application.Features.Users.Commands;

using Forex.Application.Commons.Exceptions;
using Forex.Application.Commons.Interfaces;
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
    List<UpdateUserAccountDto> Accounts)
    : IRequest<bool>;

public record UpdateUserAccountDto
{
    public long CurrencyId { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal Discount { get; set; }
}

public class UpdateUserCommandHandler(
    IAppDbContext context)
    : IRequestHandler<UpdateUserCommand, bool>
{
    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), nameof(request.Id), request.Id);

        // Asosiy user ma'lumotlarini yangilash
        user.Name = request.Name;
        user.Phone = request.Phone;
        user.Email = request.Email;
        user.Role = request.Role;
        user.Address = request.Address;
        user.Description = request.Description;

        if (!string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = request.Password;

        // Accountlarni yangilash yoki yaratish
        if (request.Accounts != null && request.Accounts.Count > 0)
        {
            foreach (var accountDto in request.Accounts)
            {
                // Mavjud accountni topish (CurrencyId bo'yicha)
                var existingAccount = user.Accounts
                    .FirstOrDefault(a => a.CurrencyId == accountDto.CurrencyId);

                if (existingAccount != null)
                {
                    // Mavjud accountni yangilash
                    existingAccount.OpeningBalance = accountDto.OpeningBalance;
                    existingAccount.Balance = accountDto.OpeningBalance; // Balansni ham yangilash
                    existingAccount.Discount = accountDto.Discount;
                }
                else
                {
                    // Yangi account yaratish
                    user.Accounts.Add(new UserAccount
                    {
                        UserId = user.Id,
                        CurrencyId = accountDto.CurrencyId,
                        OpeningBalance = accountDto.OpeningBalance,
                        Balance = accountDto.OpeningBalance,
                        Discount = accountDto.Discount
                    });
                }
            }
        }

        context.Users.Update(user);
        return await context.SaveAsync(cancellationToken);
    }
}