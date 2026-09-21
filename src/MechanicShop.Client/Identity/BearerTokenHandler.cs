using System.Net;
using System.Net.Http.Headers;

namespace MechanicShop.Client.Identity;

public class BearerTokenHandler(IAccountManagement accountManagement) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var requestPath = request.RequestUri?.AbsolutePath ?? string.Empty;
        if (requestPath.Contains("identity/token", StringComparison.OrdinalIgnoreCase))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var authResult = await accountManagement.LoadAccessTokenFromStorageAsync();

        if (authResult?.AccessToken is null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            authResult.AccessToken);

        var response = await base.SendAsync(request, cancellationToken);

        if (
            response.StatusCode == HttpStatusCode.Unauthorized
            && !request.Headers.Contains("X-Retry")
        )
        {
            var newTokenResponse = await accountManagement.RefreshTokenAsync();

            if (newTokenResponse is not null)
            {
                var newRequest = await CloneRequestAsync(request);
                newRequest.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    newTokenResponse.AccessToken);
                newRequest.Headers.Add("X-Retry", "true");

                return await base.SendAsync(newRequest, cancellationToken);
            }

            await accountManagement.LogoutAsync();
        }

        return response;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
        };

        // Copy the request content if it exists
        if (request.Content != null)
        {
            var memoryStream = new MemoryStream();
            await request.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            clone.Content = new StreamContent(memoryStream);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // Copy the request headers
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
