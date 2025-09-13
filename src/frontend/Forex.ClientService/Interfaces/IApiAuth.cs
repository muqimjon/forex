namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Auths;
using Forex.ClientService.Models.Commons;
using Refit;

public interface IApiAuth
{
    [Post("/Auth/login")]
    Task<Response<LoginResponse>> Login([Body] LoginRequest request);

    [Post("/Auth/register")]
    Task<Response<LoginResponse>> Register([Body] RegisterRequest request);
}
