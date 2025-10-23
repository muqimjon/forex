namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Refit;

public interface IApiSemiProductEntry
{
    [Post("/semi-product-entries")]
    Task<Response<long>> Create(SemiProductIntakeRequest content);

    [Delete("/semi-product-entries/{invoiceId}")]
    Task<Response<bool>> Delete(long invoiceId);
}