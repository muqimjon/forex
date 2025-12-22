namespace Forex.Infrastructure.Identity;

using Forex.Application.Commons.Interfaces;
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
}