using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace BlazorWebApplication.Server.Endpoints;

/// <summary>
/// Sign-in and sign-out URLs for the app's menu:
///   GET  /authentication/login?returnUrl=...  sends the browser to Microsoft Entra ID.
///   POST /authentication/logout               signs out of the app and of Entra ID.
/// Logout is a POST with an antiforgery token, so another site can't sign you out with a simple link.
/// </summary>
public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/authentication")
            .ExcludeFromDescription();

        group.MapGet("/login", (HttpRequest request, string? returnUrl) =>
            TypedResults.Challenge(GetAuthProperties(request, returnUrl)))
            .AllowAnonymous();

        // [FromForm] makes minimal APIs check the antiforgery token (form field __RequestVerificationToken).
        group.MapPost("/logout", (HttpRequest request, [FromForm] string? returnUrl) =>
            TypedResults.SignOut(GetAuthProperties(request, returnUrl),
                [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]));

        return app;
    }

    // Only allow return URLs inside this app. Anything else (e.g. https://evil.example) goes to the home page.
    private static AuthenticationProperties GetAuthProperties(HttpRequest request, string? returnUrl)
    {
        var baseUri = new Uri($"{request.Scheme}://{request.Host}{request.PathBase}/");

        if (!Uri.TryCreate(baseUri, returnUrl, out var redirectUri)
            || !(redirectUri.GetLeftPart(UriPartial.Path) + "/").StartsWith(baseUri.AbsoluteUri, StringComparison.Ordinal))
        {
            redirectUri = baseUri;
        }

        return new AuthenticationProperties { RedirectUri = redirectUri.AbsoluteUri };
    }
}
