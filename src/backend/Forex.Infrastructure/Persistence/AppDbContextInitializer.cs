namespace Forex.Infrastructure.Persistence;

using Forex.Application.Commons.Interfaces;
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
        if (!await context.Users.AnyAsync(u => u.Username == "admin"))
        {
            var admin = new User
            {
                Name = "System Admin",
                Username = "admin",
                Email = "admin@forex.uz",
                Role = UserRole.Hodim, 
                PasswordHash = hasher.HashPassword("741"),
                NormalizedName = "SYSTEM ADMIN"
            };

            context.Users.Add(admin);
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