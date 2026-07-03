namespace Forex.Application.Common.Interfaces;

public interface ICurrentUser
{
    long? UserId { get; }
    string? Username { get; }

    // Token ichidagi "perm" claim'idan olingan bo'lim ruxsatlari bitmask'i (yo'q bo'lsa 0).
    long Permissions { get; }
}