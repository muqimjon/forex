namespace Forex.Infrastructure.Identity;

using Forex.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public long? UserId
    {
        get
        {
            var idClaim = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(idClaim, out var id) ? id : null;
        }
    }

    // Token ichidagi Name claim'idan loginni oladi (masalan: "admin")
    public string? Username => User?.FindFirstValue("username");

    // Token ichidagi "perm" claim'idan bo'lim ruxsatlari bitmask'ini oladi (yo'q bo'lsa 0).
    public long Permissions =>
        long.TryParse(User?.FindFirstValue("perm"), out var mask) ? mask : 0;
}