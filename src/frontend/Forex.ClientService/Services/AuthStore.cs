namespace Forex.ClientService.Services;

using Forex.ClientService.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class AuthStore : INotifyPropertyChanged
{
    private string token = string.Empty;
    private string fullName = string.Empty;
    private string username = string.Empty; // Yangi maydon
    private long userId;
    private AccessPermissions permissions = AccessPermissions.None;

    public string Token
    {
        get => token;
        private set
        {
            token = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsAuthenticated));
        }
    }

    public string FullName
    {
        get => fullName;
        private set
        {
            fullName = value;
            OnPropertyChanged();
        }
    }

    // Adminlikni tekshirish uchun juda muhim
    public string Username
    {
        get => username;
        private set
        {
            username = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsAdmin)); // Adminlik o'zgarganda UI xabar topishi uchun
            RaisePermissionFlags();
        }
    }

    public long UserId
    {
        get => userId;
        private set
        {
            userId = value;
            OnPropertyChanged();
        }
    }

    // Bo'lim ruxsatlari bitmask'i (login javobidan keladi). admin doim All hisoblanadi.
    public AccessPermissions Permissions
    {
        get => IsAdmin ? AccessPermissions.All : permissions;
        private set
        {
            permissions = value;
            OnPropertyChanged();
            RaisePermissionFlags();
        }
    }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

    // UI-da Admin panellarini ko'rsatish/yashirish uchun qulay helper
    public bool IsAdmin => Username?.ToLower() == "admin";

    // Berilgan bo'limga ruxsat bor-yo'qligini tekshiradi (admin doim true).
    public bool Has(AccessPermissions section) => (Permissions & section) == section;

    // Har bir menyu bo'limi uchun qulay bind qilinadigan bayroqlar.
    public bool CanSales => Has(AccessPermissions.Sales);
    public bool CanReturns => Has(AccessPermissions.Returns);
    public bool CanPayments => Has(AccessPermissions.Payments);
    public bool CanProducts => Has(AccessPermissions.Products);
    public bool CanBarcode => Has(AccessPermissions.Barcode);
    public bool CanSupply => Has(AccessPermissions.Supply);
    public bool CanUsers => Has(AccessPermissions.Users);
    public bool CanReports => Has(AccessPermissions.Reports);
    public bool CanSettings => Has(AccessPermissions.Settings);

    // Boshqaruv paneli (pul/savdo aylanmasi) — Hisobot yoki Savdo ruxsati bo'lganda ko'rinadi.
    public bool CanDashboard => CanReports || CanSales;

    // SetAuth metodiga username va ruxsat mask parametrini qo'shdik
    public void SetAuth(string token, string fullName, string username, long userId, long accessMask = 0)
    {
        Token = token;
        FullName = fullName;
        Username = username;
        UserId = userId;
        Permissions = (AccessPermissions)accessMask;
    }

    public void Logout()
    {
        Token = string.Empty;
        FullName = string.Empty;
        Username = string.Empty;
        UserId = 0;
        Permissions = AccessPermissions.None;
    }

    public static readonly AuthStore Instance = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string name = null!)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Ruxsat yoki adminlik o'zgarganda barcha Can* bayroqlarini yangilaydi.
    private void RaisePermissionFlags()
    {
        OnPropertyChanged(nameof(Permissions));
        OnPropertyChanged(nameof(CanSales));
        OnPropertyChanged(nameof(CanReturns));
        OnPropertyChanged(nameof(CanPayments));
        OnPropertyChanged(nameof(CanProducts));
        OnPropertyChanged(nameof(CanBarcode));
        OnPropertyChanged(nameof(CanSupply));
        OnPropertyChanged(nameof(CanUsers));
        OnPropertyChanged(nameof(CanReports));
        OnPropertyChanged(nameof(CanSettings));
        OnPropertyChanged(nameof(CanDashboard));
    }
}
