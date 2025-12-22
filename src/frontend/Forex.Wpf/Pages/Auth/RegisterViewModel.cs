namespace Forex.Wpf.Pages.Auth;

using Forex.ClientService.Extensions;
using Forex.ClientService.Interfaces;
using Forex.ClientService.Services;
using Forex.Wpf.Pages.Common;

public class RegisterViewModel(IApiAuth apiAuth) : ViewModelBase
{
    public async Task<bool> RegisterAsync(string name, string email, string phone, string password, string confirm)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone)) ||
            string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Ism, telefon raqam va parol majburiy.";
            return false;
        }

        if (password != confirm)
        {
            ErrorMessage = "Parol tasdiqlanmadi";
            return false;
        }

        var resp = await apiAuth.Register(new()
        {
            Name = name,
            Email = email,
            Phone = phone,
            Password = password
        }).Handle(isLoading => IsLoading = isLoading);

        if (resp.StatusCode != 200 || resp.Data is null)
        {
            ErrorMessage = resp.Message ?? "Ro‘yxatdan o‘tish muvaffaqiyatsiz.";
            return false;
        }

        var loginResp = resp.Data;
        //AuthStore.Instance.SetAuth(loginResp.Token, loginResp.User.Name, loginResp.User.Id);
        SuccessMessage = $"{AuthStore.Instance.FullName}, Forex tizimiga muvaffaqiyatli kirildi";

        return true;
    }
}
