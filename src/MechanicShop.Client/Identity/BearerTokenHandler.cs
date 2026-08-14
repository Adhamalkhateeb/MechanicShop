using System.Net;
using System.Net.Http.Headers;

namespace MechanicShop.Client.Identity;

public class BearerTokenHandler(IAccountManagement accountManagement) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var authResult = await accountManagement.LoadAccessTokenFromStorageAsync();

        if (authResult?.AccessToken is null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            authResult.AccessToken
        );

        var response = await base.SendAsync(request, cancellationToken);

        if (
            response.StatusCode == HttpStatusCode.Unauthorized
            && !request.Headers.Contains("X-Retry")
        )
        {
            var newTokenResponse = await accountManagement.RefreshTokenAsync();

            if (newTokenResponse is not null)
            {
                var newRequest = CloneRequest(request);
                newRequest.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    newTokenResponse.AccessToken
                );
                newRequest.Headers.Add("X-Retry", "true");

                return await base.SendAsync(newRequest, cancellationToken);
            }

            await accountManagement.LogoutAsync();
        }

        return response;
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
        };

        // Copy the request content if it exists
        if (request.Content != null)
        {
            var memoryStream = new MemoryStream();
            request.Content.CopyToAsync(memoryStream).Wait();
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
