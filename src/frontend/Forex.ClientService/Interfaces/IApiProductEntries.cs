namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Refit;

public interface IApiProductEntries
{
    [Post("/product-entries")]
    Task<Response<long?>> Create(CreateProductEntryCommandRequest request);

    [Put("/product-entries")]
    Task<Response<bool>> Update(ProductEntryRequest request);

    [Delete("/product-entries/{id}")]
    Task<Response<bool>> Delete(long id);

    [Post("/product-entries/filter")]
    Task<Response<List<ProductEntryResponse>>> Filter(FilteringRequest request);
}
