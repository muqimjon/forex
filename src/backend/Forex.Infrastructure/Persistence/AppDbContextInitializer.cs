namespace Forex.Infrastructure.Persistence;

using Forex.Application.Common.Interfaces;
using Forex.Domain.Entities;
using Forex.Domain.Enums;
using Microsoft.EntityFrameworkCore;

public static class AppDbContextInitializer
{
    public static async Task SeedDataAsync(IAppDbContext context, IPasswordHasher hasher)
    {
        // 1. Agar baza bo'lmasa, yaratadi va migrationlarni yurgizadi
        // (Agar WebApi-da buni avtomat qilmoqchi bo'lsangiz)
        // await context.Database.MigrateAsync();

        // 2. Admin foydalanuvchisini qo'shish
        var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        if (existingAdmin is null)
        {
            var admin = new User
            {
                Name = "System Admin",
                Username = "admin",
                Email = "admin@forex.uz",
                Role = UserRole.Hodim,
                PasswordHash = hasher.HashPassword("741"),
                NormalizedName = "SYSTEM ADMIN",
                AccessMask = (long)AccessPermissions.All
            };

            context.Users.Add(admin);
            await context.SaveAsync(default);
        }
        else if (existingAdmin.AccessMask != (long)AccessPermissions.All)
        {
            // Mavjud admin barcha bo'limlarga ega bo'lishini kafolatlaymiz.
            existingAdmin.AccessMask = (long)AccessPermissions.All;
            await context.SaveAsync(default);
        }

        // 3. Agar kerak bo'lsa, boshqa boshlang'ich ma'lumotlar (masalan, valyutalar)
        if (!await context.Currencies.AnyAsync())
        {
            context.Currencies.AddRange(
                new Currency { Name = "Dollar", Symbol = "$", Code = "USD" },
                new Currency { Name = "So'm", Symbol = "UZS", Code = "UZS" }
            );
            await context.SaveAsync(default);
        }
    }
}