namespace Forex.Application.Commons.Interfaces;

public interface ICurrentUser
{
    string? UserId { get; }
    string? Email { get; }
    Task<IList<string>> GetRolesAsync();
}