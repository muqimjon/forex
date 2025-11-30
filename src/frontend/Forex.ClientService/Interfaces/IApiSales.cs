namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Refit;

public interface IApiSales
{
    [Post("/sales")]
    Task<Response<long?>> Create(SaleRequest request);

    [Delete("/sales/{id}")]
    Task<Response<bool>> Delete(long id);

    [Post("/sales/filter")]
    Task<Response<List<SaleResponse>>> Filter(FilteringRequest request);
    [Get("/sales")]
    Task<Response<List<SaleResponse>>> GetAll();
}
