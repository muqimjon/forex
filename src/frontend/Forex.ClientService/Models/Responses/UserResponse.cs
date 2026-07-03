namespace Forex.ClientService.Models.Responses;

using Forex.ClientService.Enums;
using System.Text.Json.Serialization;

public sealed record UserResponse
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
    public string? ProfileImageUrl { get; set; }

    // Bo'lim ruxsatlari bitmask'i (login javobi va GetById shuni qaytaradi).
    public long AccessMask { get; set; }

    public IList<UserAccountResponse> Accounts { get; set; } = default!;

    public long SettlementCurrencyId { get; set; }
    public CurrencyResponse? SettlementCurrency { get; set; }


    [JsonIgnore]
    public bool IsEditing { get; set; }

    [JsonIgnore]
    public UserAccountResponse? SettlementAccount =>
        Accounts?.FirstOrDefault(a => a.CurrencyId == SettlementCurrencyId) ?? Accounts?.FirstOrDefault();

    [JsonIgnore]
    public decimal? FirstBalance => SettlementAccount?.Balance;

    [JsonIgnore]
    public string? FirstCurrencyName => SettlementAccount?.Currency?.Name ?? SettlementCurrency?.Name;

    [JsonIgnore]
    public string? FirstCurrencyCode => SettlementAccount?.Currency?.Code ?? SettlementCurrency?.Code;

}