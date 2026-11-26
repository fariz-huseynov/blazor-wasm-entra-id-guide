using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BlazorWebApplication.Server.OpenApi;

/// <summary>
/// Adds a "Bearer" security scheme to the OpenAPI document and marks every operation as needing it.
/// Scalar reads this and shows a "Bearer Token" field. (The document only contains /api/v1, which all need a token.)
/// </summary>
internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public const string SchemeId = JwtBearerDefaults.AuthenticationScheme; // "Bearer"

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "An Entra ID access token for api://<client-id> (paste it without the 'Bearer ' prefix).",
        };

        var requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(SchemeId, document)] = [],
        };

        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations?.Values ?? Enumerable.Empty<OpenApiOperation>()))
        {
            operation.Security ??= [];
            operation.Security.Add(requirement);
        }

        return Task.CompletedTask;
    }
}
