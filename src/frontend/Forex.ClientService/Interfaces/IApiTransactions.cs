namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Refit;

public interface IApiTransactions
{
    [Post("/transactions")]
    Task<Response<long>> CreateAsync(TransactionRequest request);

    [Delete("/transactions/{id}")]
    Task<Response<bool>> Delete(long id);

    [Get("/transactions")]
    Task<Response<List<TransactionResponse>>> GetAll();

    [Post("/transactions/filter")]
    Task<Response<List<TransactionResponse>>> Filter(FilteringRequest request);
}