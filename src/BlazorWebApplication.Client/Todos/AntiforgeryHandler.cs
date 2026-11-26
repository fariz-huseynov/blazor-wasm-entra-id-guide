using Microsoft.AspNetCore.Components.Forms;

namespace BlazorWebApplication.Client.Todos;

/// <summary>
/// Adds the antiforgery token to every request that changes data, in the "RequestVerificationToken" header.
/// The server wrote the token into the page during prerendering; <see cref="AntiforgeryStateProvider"/> reads it back.
/// The browser sends the sign-in and antiforgery cookies by itself, because the API is on the same site.
/// </summary>
public sealed class AntiforgeryHandler(AntiforgeryStateProvider antiforgery) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get && request.Method != HttpMethod.Head
            && antiforgery.GetAntiforgeryToken() is { } token)
        {
            request.Headers.TryAddWithoutValidation("RequestVerificationToken", token.Value);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
