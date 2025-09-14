namespace Forex.ClientService.Services;

using Forex.ClientService.Configuration;
using Forex.ClientService.Interfaces;
using Forex.ClientService.Models.Auths;
using Forex.ClientService.Models.Commons;
using Refit;
using System.Threading.Tasks;

public class AuthService(string baseUrl)
{
    private readonly IApiAuth api = ApiServiceFactory.Create<IApiAuth>(baseUrl);

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var response = await api.Register(request);
        return HandleAuthResponse(response);
    }

    public async Task<(bool Success, string? Error)> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await api.Login(request);

            if (response.StatusCode != 200 || response.Data is null)
                return (false, response.Message ?? "Login muvaffaqiyatsiz.");

            var login = response.Data;
            var user = login.User;

            AuthStore.Instance.SetAuth(login.Token, user.Name, user.Id);
            return (true, null);
        }
        catch (ApiException apiEx)
        {
            try
            {
                var problem = await apiEx.GetContentAsAsync<Response<LoginResponse>>();
                return (false, problem?.Message ?? apiEx.Message);
            }
            catch
            {
                return (false, apiEx.Message);
            }
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
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
