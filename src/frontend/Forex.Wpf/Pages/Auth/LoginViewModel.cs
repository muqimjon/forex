namespace Forex.Wpf.Pages.Auth;

using Forex.ClientService.Extensions;
using Forex.ClientService.Interfaces;
using Forex.ClientService.Services;
using Forex.Wpf.Pages.Common;
using System.Threading.Tasks;

public class LoginViewModel : ViewModelBase
{
    private readonly IApiAuth apiAuth = App.Client.Auth;

    public async Task<bool> LoginAsync(string login, string password)
    {
        ErrorMessage = "";

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Login va parol majburiy.";
            return false;
        }

        var resp = await apiAuth.Login(new() { EmailOrPhone = login, Password = password })
            .Handle(isLoading => IsLoading = isLoading);

        if (resp.StatusCode != 200)
        {
            ErrorMessage = resp.Message;
            return false;
        }
        else if (resp.Data is null)
        {
            ErrorMessage = "Login muvaffaqiyatsiz.";
            return false;
        }

        var loginResp = resp.Data;
        AuthStore.Instance.SetAuth(loginResp.Token, loginResp.User.Name, loginResp.User.Id);
        SuccessMessage = $"{AuthStore.Instance.FullName}, Forex tizimiga muvaffaqiyatli kirildi";

        return true;
    }
}
