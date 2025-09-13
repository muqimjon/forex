namespace Forex.ClientService.Services;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public class AuthStore : INotifyPropertyChanged
{
    private string token = string.Empty;
    private string fullName = string.Empty;
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

    public void SetAuth(string token, string fullName, long userId)
    {
        Token = token;
        FullName = fullName;
        UserId = userId;
    }

    public void Logout()
    {
        Token = string.Empty;
        FullName = string.Empty;
        UserId = 0;
    }

    public static readonly AuthStore Instance = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string name = null!)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
