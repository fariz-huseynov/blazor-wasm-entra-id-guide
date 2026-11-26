using BlazorWebApplication.Shared.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace BlazorWebApplication.Server.Auth;

/// <summary>
/// All named authorization policies of the server, in one place.
/// Endpoints use them with <c>.RequireAuthorization(Policies.UiRead)</c>.
/// </summary>
public static class Policies
{
    /// <summary>Read todos through /api/ui: a signed-in user (cookie) with Reader or Contributor.</summary>
    public const string UiRead = "Ui.Read";

    /// <summary>Change todos through /api/ui: a signed-in user (cookie) with Contributor.</summary>
    public const string UiWrite = "Ui.Write";

    public static AuthorizationBuilder AddAppPolicies(this AuthorizationBuilder builder) => builder
        // The UI API only accepts the sign-in cookie. Naming the cookie scheme also means that a missing
        // or wrong user gets 401/403 from the cookie handler, not a redirect to the Microsoft sign-in page.
        .AddPolicy(UiRead, policy => policy
            .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .RequireRole(AppRoles.Reader, AppRoles.Contributor))
        .AddPolicy(UiWrite, policy => policy
            .AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .RequireRole(AppRoles.Contributor));
}
