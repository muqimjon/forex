namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Responses;
using Refit;

public interface IApiShops
{
    [Get("/shops")]
    Task<Response<List<ShopResponse>>> GetAllAsync();

    [Get("/shops/{id}")]
    Task<Response<ShopResponse>> GetById(long id);

    [Post("/shops")]
    Task<Response<long>> Create([Body] ShopResponse dto);

    [Put("/shops")]
    Task<Response<bool>> Update([Body] ShopResponse dto);

    [Delete("/shops/{id}")]
    Task<Response<bool>> Delete(long id);
}