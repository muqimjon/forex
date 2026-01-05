namespace Forex.Application.Common.Interfaces;

public interface ICurrentUser
{
    long? UserId { get; }
    string? Username { get; }
}