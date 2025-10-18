namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Responses;
using Refit;

public interface IApiProductTypes

{
    [Get("/product-types")]
    Task<Response<List<ProductTypeResponse>>> GetAll();
}