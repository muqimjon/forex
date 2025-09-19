namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Users;
using Refit;

public interface IApiUser
{
    [Get("/Users")]
    Task<Response<List<UserResponse>>> GetAll();

    [Post("/Users/filter")]
    Task<Response<List<UserResponse>>> Filter(FilteringRequest request);

    [Get("/Users/{id}")]
    Task<Response<UserResponse>> GetById(long id);

    [Post("/Users")]
    Task<Response<long>> Create([Body] UserRequest request);

    [Put("/Users")]
    Task<Response<bool>> Update([Body] UserRequest request);

    [Delete("/Users/{id}")]
    Task<Response<bool>> Delete(long id);
}
