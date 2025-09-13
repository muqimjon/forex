namespace Forex.ClientService.Services;

using Forex.ClientService.Configuration;
using Forex.ClientService.Interfaces;
using Forex.ClientService.Models.Auths;
using Forex.ClientService.Models.Commons;
using System.Threading.Tasks;

public class AuthService(string baseUrl)
{
    private readonly IApiAuth api = ApiServiceFactory.Create<IApiAuth>(baseUrl);

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var response = await api.Register(request);
        return HandleAuthResponse(response);
    }

    public async Task<bool> LoginAsync(LoginRequest request)
    {
        var response = await api.Login(request);
        return HandleAuthResponse(response);
    }

    private static bool HandleAuthResponse(Response<LoginResponse> response)
    {
        if (response.StatusCode != 200 || response.Data is null)
            return false;

        var login = response.Data;
        var user = login.User;

        AuthStore.Instance.SetAuth(login.Token, user.Name, user.Id);
        return true;
    }

    public static void Logout() => AuthStore.Instance.Logout();
}
