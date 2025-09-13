namespace Forex.ClientService.Configuration;

using Forex.ClientService.Services;
using Refit;
using System;
using System.Net.Http;

public static class ApiServiceFactory
{
    public static T Create<T>(string baseUrl) where T : class
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

        var token = AuthStore.Instance.Token;
        if (!string.IsNullOrWhiteSpace(token))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return RestService.For<T>(httpClient);
    }
}

