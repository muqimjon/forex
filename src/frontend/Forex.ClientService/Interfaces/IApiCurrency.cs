namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Responses;
using Refit;

public interface IApiCurrency
{
    [Get("/currencies")]
    Task<Response<List<CurrencyResponse>>> GetAll();

}
