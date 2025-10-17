namespace Forex.ClientService.Configuration;

using Forex.ClientService.Services;
using System.Net.Http;
using System.Net.Http.Headers;

public class AuthHeaderHandler : DelegatingHandler
{
    public AuthHeaderHandler() { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = AuthStore.Instance.Token;

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
