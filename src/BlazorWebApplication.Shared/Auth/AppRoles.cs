namespace BlazorWebApplication.Shared.Auth;

/// <summary>
/// The app roles defined in the Entra ID app registration (App roles → Value).
/// The values must match the portal exactly, including case.
/// </summary>
public static class AppRoles
{
    /// <summary>Can read todos. For users and applications.</summary>
    public const string Reader = "Reader";

    /// <summary>Can read and change todos in the web app. For users only.</summary>
    public const string Contributor = "Contributor";

    /// <summary>Can change todos through the external API (step 11). For users and applications.</summary>
    public const string ApiWriter = "Api.Writer";

    /// <summary>The claim type that carries the roles in Entra ID tokens.</summary>
    public const string ClaimType = "roles";
}
