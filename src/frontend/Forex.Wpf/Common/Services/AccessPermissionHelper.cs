namespace Forex.Wpf.Common.Services;

using Forex.ClientService.Enums;

/// <summary>
/// Hodim bo'lim-ruxsatlari uchun shablonlar va yordamchi funksiyalar.
/// UserPage va UserWindow ruxsat muharrirlari shu yerdan foydalanadi.
/// </summary>
public static class AccessPermissionHelper
{
    public const string Custom = "Moslashtirilgan";

    // Shablonlar (nom → mask). Tartib ComboBox'da ko'rinadigan tartib.
    public static readonly (string Name, AccessPermissions Mask)[] Presets =
    [
        ("Qadoqlovchi", AccessPermissions.Products | AccessPermissions.Barcode),
        ("Sotuvchi", AccessPermissions.Sales | AccessPermissions.Returns | AccessPermissions.Payments),
        ("Menejer", AccessPermissions.Sales | AccessPermissions.Returns | AccessPermissions.Payments
                    | AccessPermissions.Products | AccessPermissions.Barcode | AccessPermissions.Supply
                    | AccessPermissions.Reports),
        ("Admin", AccessPermissions.All),
        (Custom, AccessPermissions.None),
    ];

    public static IEnumerable<string> PresetNames => Presets.Select(p => p.Name);

    /// <summary>Mask shablonga to'liq mos kelsa shablon nomini, aks holda "Moslashtirilgan" qaytaradi.</summary>
    public static string MatchPreset(AccessPermissions mask)
    {
        foreach (var (name, m) in Presets)
            if (name != Custom && m == mask)
                return name;
        return Custom;
    }

    /// <summary>Shablon nomiga mos maskni qaytaradi; "Moslashtirilgan"/noma'lum uchun null.</summary>
    public static AccessPermissions? PresetMask(string? name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        foreach (var (n, m) in Presets)
            if (n == name && n != Custom)
                return m;
        return null;
    }
}
