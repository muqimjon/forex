namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Refit;

public interface IApiProductEntries
{
    [Post("/product-entries")]
    Task<Response<long>> Create(CreateProductEntryCommandRequest request);

    [Delete("/product-entries")]
    Task<Response<bool>> Delete(long id);
}