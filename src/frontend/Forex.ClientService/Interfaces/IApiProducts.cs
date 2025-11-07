namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Refit;

public interface IApiProducts
{
    [Get("/products")]
    Task<Response<List<ProductResponse>>> GetAll();

    [Get("/products/{id}")]
    Task<Response<ProductResponse>> GetById(long id);

    [Post("/products")]
    Task<Response<long?>> Create([Body] ProductRequest request);

    [Put("/products")]
    Task<Response<bool>> Update([Body] ProductRequest request);

    [Delete("/products/{id}")]
    Task<Response<bool>> Delete(long id);

    [Post("/products/filter")]
    Task<Response<List<ProductResponse>>> Filter(FilteringRequest request);
}
