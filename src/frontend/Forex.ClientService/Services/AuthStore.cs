namespace Forex.ClientService.Services;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public class AuthStore : INotifyPropertyChanged
{
    private string token = string.Empty;
    private string fullName = string.Empty;
    private string username = string.Empty; // Yangi maydon
    private long userId;

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

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

    // UI-da Admin panellarini ko'rsatish/yashirish uchun qulay helper
    public bool IsAdmin => Username?.ToLower() == "admin";

    // SetAuth metodiga username parametrini qo'shdik
    public void SetAuth(string token, string fullName, string username, long userId)
    {
        Token = token;
        FullName = fullName;
        Username = username;
        UserId = userId;
    }

    public void Logout()
    {
        Token = string.Empty;
        FullName = string.Empty;
        Username = string.Empty;
        UserId = 0;
    }

    public static readonly AuthStore Instance = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string name = null!)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}