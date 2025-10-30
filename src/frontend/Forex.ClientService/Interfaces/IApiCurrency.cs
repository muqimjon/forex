namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Requests;
using Forex.ClientService.Models.Responses;
using Refit;

public interface IApiCurrency
{
    [Post("/currencies")]
    Task<Response<long>> CreateAsync([Body] CurrencyRequest request);

    [Put("/currencies​")]
    Task<Response<bool>> UpdateAsync([Body] CurrencyRequest request);

    [Put("/currencies/all")]
    Task<Response<bool>> SaveAllAsync(List<CurrencyRequest> dtoList);

    [Delete("/currencies​/{id}")]
    Task<Response<bool>> DeleteAsync(long id);

    [Get("/currencies/{id}")]
    Task<Response<CurrencyResponse>> GetByIdAsync(long id);

    [Get("/currencies")]
    Task<Response<List<CurrencyResponse>>> GetAllAsync();

}
