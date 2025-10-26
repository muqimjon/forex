namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Refit;

public interface IApiTransactions
{
    [Post("/transactions")]
    Task<Response<long>> CreateAsync(TransactionRequest request);

    [Delete("/transactions/{id}")]
    Task<Response<bool>> Delete(long id);
}