namespace BlazorWebApplication.Server.Endpoints;

/// <summary>
/// The API that the app's own WebAssembly pages call. It is an internal detail of the app,
/// so it is hidden from the OpenAPI document.
/// </summary>
public static class UiTodoEndpoints
{
    public static IEndpointRouteBuilder MapUiTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ui/todos")
            .ExcludeFromDescription();

        group.MapGet("/", TodoHandlers.GetAll);
        group.MapGet("/{id:guid}", TodoHandlers.GetById);
        group.MapPost("/", TodoHandlers.Create);
        group.MapPut("/{id:guid}", TodoHandlers.Update);
        group.MapDelete("/{id:guid}", TodoHandlers.Delete);

        return app;
    }
}
