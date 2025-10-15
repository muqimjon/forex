namespace Forex.ClientService;

using Forex.ClientService.Configuration;
using Forex.ClientService.Interfaces;

public class ForexClient(string baseUrl)
{
    public IApiAuth Auth { get; } = ApiServiceFactory.Create<IApiAuth>(baseUrl);
    public IApiUser Users { get; } = ApiServiceFactory.Create<IApiUser>(baseUrl);
    public IApiSemiProduct SemiProduct { get; } = ApiServiceFactory.Create<IApiSemiProduct>(baseUrl);
    public IApiManufactory Manufactories { get; } = ApiServiceFactory.Create<IApiManufactory>(baseUrl);
    public IApiCurrency Currency { get; } = ApiServiceFactory.Create<IApiCurrency>(baseUrl);
    public IApiUnitMeasure UnitMeasure { get; } = ApiServiceFactory.Create<IApiUnitMeasure>(baseUrl);
    public IApiProducts Products { get; } = ApiServiceFactory.Create<IApiProducts>(baseUrl);
}
