using BlazorWebApplication.Server.Auth;

namespace BlazorWebApplication.Server.Endpoints;

/// <summary>
/// The API that the app's own WebAssembly pages call. It is an internal detail of the app,
/// so it is hidden from the OpenAPI document.
/// It only accepts the sign-in cookie: readers may read, contributors may also change todos.
/// </summary>
public static class UiTodoEndpoints
{
    public static IEndpointRouteBuilder MapUiTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ui/todos")
            .ExcludeFromDescription()
            .RequireAuthorization(Policies.UiRead)
            .AddEndpointFilter<AntiforgeryEndpointFilter>();

        group.MapGet("/", TodoHandlers.GetAll);
        group.MapGet("/{id:guid}", TodoHandlers.GetById);

        // These endpoints need both policies: UiRead from the group and UiWrite.
        group.MapPost("/", TodoHandlers.Create).RequireAuthorization(Policies.UiWrite);
        group.MapPut("/{id:guid}", TodoHandlers.Update).RequireAuthorization(Policies.UiWrite);
        group.MapDelete("/{id:guid}", TodoHandlers.Delete).RequireAuthorization(Policies.UiWrite);

        return app;
    }
}
