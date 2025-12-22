namespace Forex.Application.Commons.Interfaces;

public interface ICurrentUser
{
    long? UserId { get; }
    string? Username { get; }
}