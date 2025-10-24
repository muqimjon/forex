namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Responses;
using Refit;

public interface IApiProductTypeItems

{
    [Get("/product-type-items")]
    Task<Response<List<ProductTypeItemResponse>>> GetAll();

    [Post("/product-type-items/filter")]
    Task<Response<List<ProductTypeItemResponse>>> Filter(FilteringRequest request);
}