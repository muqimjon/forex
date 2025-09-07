namespace Forex.Infrastructure.Identity;

using Forex.Application.Commons.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private readonly ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;

    public string? UserId => user?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email => user?.FindFirstValue(ClaimTypes.Email);

    public Task<IList<string>> GetRolesAsync()
    {
        IList<string> roles = user?
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? [];

        return Task.FromResult(roles);
    }
}
