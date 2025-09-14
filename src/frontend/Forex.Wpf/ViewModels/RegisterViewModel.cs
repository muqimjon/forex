namespace Forex.Wpf.ViewModels;

using Forex.ClientService;
using Forex.ClientService.Extensions;
using Forex.ClientService.Interfaces;
using Forex.ClientService.Services;
using Forex.Wpf.Pages.Common;
using System.Threading.Tasks;

public class RegisterViewModel(ForexClient client) : ViewModelBase
{
    private readonly IApiAuth apiAuth = client.Auth;

    public async Task<bool> RegisterAsync(string name, string email, string phone, string password, string confirm)
    {
        ErrorMessage = "";

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone) ||
            string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Name, contact va password majburiy.";
            return false;
        }

        if (password != confirm)
        {
            ErrorMessage = "Passwords do not match.";
            return false;
        }

        var resp = await apiAuth.Register(new()
        {
            Name = name,
            Email = email,
            Phone = phone,
            Password = password
        }).Handle();

        if (resp.StatusCode != 200 || resp.Data is null)
        {
            ErrorMessage = resp.Message ?? "Registration failed.";
            return false;
        }

        // ✅ Agar muvaffaqiyatli bo‘lsa, darhol login qilamiz
        var loginResp = resp.Data;
        AuthStore.Instance.SetAuth(loginResp.Token, loginResp.User.Name, loginResp.User.Id);

        return true;
    }
}
