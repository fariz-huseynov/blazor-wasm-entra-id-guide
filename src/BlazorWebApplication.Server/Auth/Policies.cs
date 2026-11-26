using System.Security.Claims;
using BlazorWebApplication.Shared.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

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

    /// <summary>Read todos through /api/v1: a bearer token with Reader or Api.Writer.</summary>
    public const string ApiRead = "Api.Read";

    /// <summary>Change todos through /api/v1: a bearer token with Api.Writer.</summary>
    public const string ApiWrite = "Api.Write";

    /// <summary>The scope that a user's (delegated) token must carry. Defined under "Expose an API".</summary>
    public const string RequiredScope = "access_as_user";

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
            .RequireRole(AppRoles.Contributor))
        // The external API only accepts bearer tokens ("Bearer" scheme). The sign-in cookie is ignored there.
        .AddPolicy(ApiRead, policy => policy
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .RequireRole(AppRoles.Reader, AppRoles.ApiWriter)
            .RequireAssertion(context => UserTokenHasScope(context.User)))
        .AddPolicy(ApiWrite, policy => policy
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .RequireRole(AppRoles.ApiWriter)
            .RequireAssertion(context => UserTokenHasScope(context.User)));

    /// <summary>
    /// A token for a user (delegated) has a "scp" claim and must contain <see cref="RequiredScope"/>.
    /// A token for an application (app-only) has no "scp" claim; it is judged by its roles alone.
    /// </summary>
    public static bool UserTokenHasScope(ClaimsPrincipal user)
    {
        // The bearer handler renames "scp" to the long ClaimConstants.Scope URI, so look for both names.
        var scopes = user.FindAll(ClaimConstants.Scp)
            .Concat(user.FindAll(ClaimConstants.Scope))
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToList();

        return scopes.Count == 0 || scopes.Contains(RequiredScope);
    }
}
