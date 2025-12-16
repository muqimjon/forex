namespace Forex.Domain.Entities;

using Forex.ClientService.Enums;
using Forex.Domain.Commons;
using Forex.Domain.Entities.Sales;
using Forex.Domain.Enums;
using System.Collections.Generic;

public class User : Auditable
{
    // Identifikatsiya
    public string Name { get; set; } = string.Empty;
    public string? NormalizedName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public UserRole Role { get; set; }

    // Profil
    public string? Address { get; set; }
    public string? Description { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? LanguagePreference { get; set; }
    public string? TimeZone { get; set; }

    // Autentifikatsiya
    public string? PasswordHash { get; set; }
    public string? Username { get; set; }
    public bool IsActive { get; set; } = true;
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int FailedLoginAttempts { get; set; }

    // Client Portal
    public string? ClientType { get; set; } // Individual / Company
    public KYCStatus Status { get; set; } // Pending / Verified / Rejected
    public DateTime? VerifiedAt { get; set; }
    public string? PreferencesJson { get; set; } // flexible storage for UI settings

    // Bog‘lanishlar
    public ICollection<UserAccount> Accounts { get; set; } = [];
    public ICollection<Sale> Sales { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<Invoice> Invoices { get; set; } = [];
    public ICollection<UserNotification> Notifications { get; set; } = [];
}
