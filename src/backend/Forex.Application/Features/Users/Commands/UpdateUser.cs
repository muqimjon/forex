namespace Forex.Application.Features.Users.Commands;

using Forex.Application.Common.Exceptions;
using Forex.Application.Common.Extensions; // .ToNormalized() uchun
using Forex.Application.Common.Interfaces;
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
    string? TempImagePath,
    long AccessMask,
    List<UpdateUserAccountDto> Accounts) : IRequest<bool>;

public class UpdateUserCommandHandler(
    IAppDbContext context,
    IPasswordHasher hasher,
    ICurrentUser currentUser,
    IFileStorageService fileStorage)
    : IRequestHandler<UpdateUserCommand, bool>
{
    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .Include(u => u.Accounts)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User));

        bool isAdmin = currentUser.Username == "admin";

        if (!isAdmin && currentUser.UserId != request.Id)
            throw new ForbiddenException("Sizda bu foydalanuvchini o'zgartirish huquqi yo'q!");

        if (!string.IsNullOrWhiteSpace(request.Username) && !isAdmin)
            throw new AppException($"Sizda {request.Username}ni o'zgartirish huquqi yo'q!");

        var normalizedNewName = request.Name.ToNormalized();
        if (user.NormalizedName != normalizedNewName)
        {
            var nameExists = await context.Users.AnyAsync(u => u.NormalizedName == normalizedNewName && u.Id != user.Id, cancellationToken);
            if (nameExists) throw new AlreadyExistException("Ushbu ismli foydalanuvchi allaqachon mavjud.");

            user.Name = request.Name;
            user.NormalizedName = normalizedNewName;
        }

        user.Phone = request.Phone;
        user.Email = request.Email;
        user.NormalizedEmail = request.Email?.ToNormalized();
        user.Address = request.Address;
        user.Description = request.Description;

        if (isAdmin)
        {
            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                var loginExists = await context.Users.AnyAsync(u => u.Username == request.Username && u.Id != user.Id, cancellationToken);
                if (loginExists) throw new AppException("Bu login allaqachon band!");
            }
            user.Username = request.Username;

            // Bo'lim ruxsatlarini faqat admin o'zgartira oladi (login/parol kabi).
            user.AccessMask = request.AccessMask;
        }

        // Image handling - same as Product logic
        var oldImagePath = user.ProfileImageUrl;
        var newImagePath = request.TempImagePath;
        string? imagePathToDelete = null;

        if (fileStorage.IsTempKey(newImagePath))
        {
            var movedKey = await fileStorage.MoveFileAsync(newImagePath!, "users", cancellationToken);
            user.ProfileImageUrl = movedKey ?? newImagePath;
            imagePathToDelete = oldImagePath;
        }
        else if (string.IsNullOrWhiteSpace(newImagePath) && !string.IsNullOrWhiteSpace(oldImagePath))
        {
            imagePathToDelete = oldImagePath;
            user.ProfileImageUrl = null;
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = hasher.HashPassword(request.Password);
        }

        context.Users.Update(user);
        var result = await context.SaveAsync(cancellationToken);

        if (result && !string.IsNullOrWhiteSpace(imagePathToDelete))
        {
            await fileStorage.DeleteFileAsync(imagePathToDelete, cancellationToken);
        }

        return result;
    }
}