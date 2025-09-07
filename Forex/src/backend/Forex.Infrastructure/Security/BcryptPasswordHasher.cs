namespace Forex.Infrastructure.Security;

using BCrypt.Net;
using Forex.Application.Commons.Interfaces;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
        => BCrypt.HashPassword(password, 12);

    public bool VerifyPassword(string hashedPassword, string providedPassword)
        => BCrypt.Verify(providedPassword, hashedPassword);
}