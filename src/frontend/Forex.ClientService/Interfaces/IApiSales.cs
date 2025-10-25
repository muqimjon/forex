namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Refit;

public interface IApiSales
{
    [Post("/sales")]
    Task<Response<long>> Create(SaleRequest request);

    [Delete("/sales")]
    Task<Response<bool>> Delete(long id);
}