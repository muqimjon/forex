namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Users;
using Refit;

public interface IApiUser
{
    [Get("/Users")]
    Task<Response<List<UserDto>>> GetAll();

    [Get("/Users/{id}")]
    Task<Response<UserDto>> GetById(long id);

    [Post("/Users")]
    Task<Response<long>> Create([Body] CreateUserRequest request);

    [Put("/Users")]
    Task<Response<bool>> Update([Body] UpdateUserRequest request);

    [Delete("/Users/{id}")]
    Task<Response<bool>> Delete(long id);
}
