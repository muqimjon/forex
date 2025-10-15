namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Refit;

public interface IApiAuth
{
    [Post("/auth/login")]
    Task<Response<LoginResponse>> Login([Body] LoginRequest request);

    [Post("/auth/register")]
    Task<Response<LoginResponse>> Register([Body] RegisterRequest request);
}
