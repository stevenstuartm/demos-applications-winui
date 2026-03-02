using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.Http;

namespace demos_applications_winui.Logging;

/// <summary>
/// Custom <see cref="IHttpClient"/> for Serilog.Sinks.Http that injects a bearer
/// token on every request using <see cref="ILogAuthTokenProvider"/>. When no token
/// is available (user not authenticated), requests are sent without authorization
/// so that log shipping is never blocked by auth state.
/// </summary>
public class AuthenticatedHttpClient(ILogAuthTokenProvider tokenProvider) : IHttpClient
{
    private readonly HttpClient _httpClient = new();

    public void Configure(IConfiguration configuration)
    {
        // No static configuration required — token is resolved dynamically per request.
    }

    public async Task<HttpResponseMessage> PostAsync(
        string requestUri,
        Stream contentStream,
        CancellationToken cancellationToken)
    {
        using var content = new StreamContent(contentStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Content = content;

        var token = tokenProvider.GetToken();
        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
