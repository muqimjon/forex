namespace Forex.ClientService.Models.Requests;

using Forex.ClientService.Enums;

public sealed record UserRequest
{

    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public UserRole Role { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public string? Password { get; set; }
    public string? TempImagePath { get; set; }

    // Bo'lim ruxsatlari bitmask'i (faqat admin o'zgartira oladi).
    public long AccessMask { get; set; }

    public List<UserAccount> Accounts { get; set; } = default!;
}