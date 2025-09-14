namespace Forex.ClientService.Configuration;

using Refit;
using System.Net.Http;

public static class ApiServiceFactory
{
    public static T Create<T>(string baseUrl)
    {
        var httpClient = new HttpClient(new AuthHeaderHandler())
        {
            BaseAddress = new Uri(baseUrl)
        };

        return RestService.For<T>(httpClient);
    }
}
