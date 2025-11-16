namespace Forex.Domain.Entities;

using Forex.Domain.Commons;

public class CompanyInfo : Auditable
{
    // I. Asosiy Ma'lumotlar
    public string Name { get; set; } = default!;
    public string Slogan { get; set; } = string.Empty;
    public string AboutUsContent { get; set; } = string.Empty;
    public string Mission { get; set; } = string.Empty;
    public string Vision { get; set; } = string.Empty;
    public DateTime EstablishedDate { get; set; }

    // II. Kontakt Ma'lumotlari
    public string PrimaryEmail { get; set; } = string.Empty;
    public string PrimaryPhone { get; set; } = string.Empty;
    public string SecondaryPhone { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string WorkingHoursText { get; set; } = string.Empty;

    // III. Huquqiy Ma'lumotlar
    public string LegalForm { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string DefaultCurrencySymbol { get; set; } = "UZS";

    // IV. Media
    public string LogoUrl { get; set; } = string.Empty;

    // V. Yillik Ma'lumotlar
    public int? EmployeesCount { get; set; }
    public int? ProjectCount { get; set; }
    public int? CustomerCount { get; set; }

    // VI. Navigatsiya (Ijtimoiy tarmoqlar)
    public ICollection<SocialLink> SocialLinks { get; set; } = [];
}
