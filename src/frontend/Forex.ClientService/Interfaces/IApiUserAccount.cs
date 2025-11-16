namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Refit;

public interface IApiUserAccount
{
    [Get("/user-account")]
    Task<Response<List<UserAccountResponse>>> GetAllAsync();

    [Put("/user-account")]
    Task<Response<bool>> UpdateAsync(UserAccountRequest request);
}