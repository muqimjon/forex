namespace Forex.Wpf.Pages.Auth;

using Forex.ClientService.Extensions;
using Forex.ClientService.Interfaces;
using Forex.ClientService.Services;
using Forex.Wpf.Pages.Common;

public class LoginViewModel(IApiAuth apiAuth) : ViewModelBase
{
    public bool LastFailureWasConnectionError { get; private set; }

    public async Task<bool> LoginAsync(string login, string password)
    {
        LastFailureWasConnectionError = false;
        //if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        //{
        //    ErrorMessage = "Login va parol majburiy.";
        //    return false;
        //}

        var resp = await apiAuth.Login(new() { Username = login, Password = password })
            .Handle(isLoading => IsLoading = isLoading);

        if (resp.StatusCode != 200)
        {
            // URL / ulanish muammosi: tarmoq/SSL/timeout (0), noto'g'ri yo'l (404),
            // noto'g'ri method (405), server/gateway xatosi (5xx). 400/401 = login xato (oyna ochilmaydi).
            LastFailureWasConnectionError = resp.StatusCode is 0 or 404 or 405 || resp.StatusCode >= 500;
            ErrorMessage = resp.Message;
            return false;
        }
        else if (resp.Data is null)
        {
            ErrorMessage = "Login muvaffaqiyatsiz.";
            return false;
        }

        var loginResp = resp.Data;

        AuthStore.Instance.SetAuth(
            loginResp.Token,
            loginResp.User.Name,
            login,
            loginResp.User.Id,
            loginResp.User.AccessMask);

        SuccessMessage = $"{AuthStore.Instance.FullName}, Forex tizimiga muvaffaqiyatli kirildi";
        return true;
    }
}
