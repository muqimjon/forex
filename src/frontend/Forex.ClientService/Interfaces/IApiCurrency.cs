namespace Forex.ClientService.Interfaces;

using Forex.ClientService.Models.Commons;
using Forex.ClientService.Models.Currencies;
using Refit;

public interface IApiCurrency
{
    [Get("/currencies")]
    Task<Response<List<CurrencyDto>>> GetAll();

}
