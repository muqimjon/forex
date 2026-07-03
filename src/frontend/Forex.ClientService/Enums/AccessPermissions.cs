namespace Forex.ClientService.Enums;

/// <summary>
/// Login qila oladigan hodimlarga beriladigan bo'lim ruxsatlari.
/// Har bir bit bitta menyu bo'limiga (HomePage tugmasiga) 1:1 mos keladi.
/// Bit qiymatlari backend (Forex.Domain.Enums.AccessPermissions) bilan AYNAN bir xil —
/// mask xom `long` bo'lib tarmoqdan o'tadi.
/// </summary>
[Flags]
public enum AccessPermissions : long
{
    None = 0,
    Sales = 1 << 0,      // 1   — Savdo
    Returns = 1 << 1,    // 2   — Qaytarish
    Payments = 1 << 2,   // 4   — To'lov
    Products = 1 << 3,   // 8   — Mahsulot
    Barcode = 1 << 4,    // 16  — Barkod
    Supply = 1 << 5,     // 32  — Ta'minot
    Users = 1 << 6,      // 64  — User
    Reports = 1 << 7,    // 128 — Hisobot
    Settings = 1 << 8,   // 256 — Sozlama

    All = Sales | Returns | Payments | Products | Barcode | Supply | Users | Reports | Settings // 511
}
