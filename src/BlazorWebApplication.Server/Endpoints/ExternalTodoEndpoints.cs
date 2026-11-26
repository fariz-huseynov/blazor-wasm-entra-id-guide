namespace BlazorWebApplication.Server.Endpoints;

/// <summary>
/// The public, versioned API for other programs (scripts, services, a console app).
/// It uses the same handlers as the UI API, but it is documented in OpenAPI
/// and will later get its own authentication (bearer tokens instead of cookies).
/// </summary>
public static class ExternalTodoEndpoints
{
    public static IEndpointRouteBuilder MapExternalTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/todos")
            .WithTags("Todos");

        group.MapGet("/", TodoHandlers.GetAll)
            .WithName("ListTodos")
            .WithSummary("List all todos")
            .WithDescription("Returns every todo, oldest first.");

        group.MapGet("/{id:guid}", TodoHandlers.GetById)
            .WithName("GetTodo")
            .WithSummary("Get a todo")
            .WithDescription("Returns one todo, or 404 if it does not exist.");

        group.MapPost("/", TodoHandlers.Create)
            .WithName("CreateTodo")
            .WithSummary("Create a todo")
            .WithDescription("Creates a todo that is not done yet. The title is required (max 200 characters).");

        group.MapPut("/{id:guid}", TodoHandlers.Update)
            .WithName("UpdateTodo")
            .WithSummary("Update a todo")
            .WithDescription("Replaces the title and the done flag of a todo.");

        group.MapDelete("/{id:guid}", TodoHandlers.Delete)
            .WithName("DeleteTodo")
            .WithSummary("Delete a todo")
            .WithDescription("Deletes a todo. Returns 404 if it does not exist.");

        return app;
    }
}
