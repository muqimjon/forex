namespace Forex.ClientService;

using Forex.ClientService.Configuration;
using Forex.ClientService.Interfaces;

public class ForexClient(string baseUrl)
{
    public IApiAuth Auth { get; } = ApiServiceFactory.Create<IApiAuth>(baseUrl);
    public IApiUser Users { get; } = ApiServiceFactory.Create<IApiUser>(baseUrl);
    public IApiSemiProduct SemiProduct { get; } = ApiServiceFactory.Create<IApiSemiProduct>(baseUrl);
}
