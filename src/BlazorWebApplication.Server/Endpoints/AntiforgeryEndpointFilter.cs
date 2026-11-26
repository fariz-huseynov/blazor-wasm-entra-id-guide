using Microsoft.AspNetCore.Antiforgery;

namespace BlazorWebApplication.Server.Endpoints;

/// <summary>
/// Checks the antiforgery token on requests that change data (POST, PUT, PATCH, DELETE).
/// Minimal APIs only check it by themselves for form posts ([FromForm]); JSON endpoints need this filter.
/// The browser sends the token in the "RequestVerificationToken" header (see the Client's AntiforgeryHandler).
/// </summary>
internal sealed class AntiforgeryEndpointFilter(IAntiforgery antiforgery) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        var method = http.Request.Method;

        if (!HttpMethods.IsGet(method) && !HttpMethods.IsHead(method) && !HttpMethods.IsOptions(method)
            && !await antiforgery.IsRequestValidAsync(http))
        {
            return TypedResults.Problem("Invalid or missing antiforgery token.", statusCode: StatusCodes.Status400BadRequest);
        }

        return await next(context);
    }
}
