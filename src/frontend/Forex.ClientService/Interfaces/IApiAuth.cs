namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Refit;

public interface IApiAuth
{
    [Get("/api/auth/ping")]
    Task<Response<string>> Ping();

    [Post("/api/auth/login")]
    Task<Response<LoginResponse>> Login([Body] LoginRequest request);
}
