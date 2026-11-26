using BlazorWebApplication.Server.Auth;
using Microsoft.AspNetCore.Mvc;

namespace BlazorWebApplication.Server.Endpoints;

/// <summary>
/// The public, versioned API for other programs (scripts, services, a console app).
/// It uses the same handlers as the UI API, but it is documented in OpenAPI
/// and only accepts bearer tokens: Reader or Api.Writer may read, Api.Writer may also change todos.
/// </summary>
public static class ExternalTodoEndpoints
{
    public static IEndpointRouteBuilder MapExternalTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/todos")
            .WithTags("Todos")
            .RequireAuthorization(Policies.ApiRead)
            // Shown in the OpenAPI document: 401 = no valid token, 403 = valid token without the needed role or scope.
            .WithMetadata(
                new ProducesResponseTypeAttribute(StatusCodes.Status401Unauthorized),
                new ProducesResponseTypeAttribute(StatusCodes.Status403Forbidden));

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
            .RequireAuthorization(Policies.ApiWrite)
            .WithSummary("Create a todo")
            .WithDescription("Creates a todo that is not done yet. The title is required (max 200 characters).");

        group.MapPut("/{id:guid}", TodoHandlers.Update)
            .WithName("UpdateTodo")
            .RequireAuthorization(Policies.ApiWrite)
            .WithSummary("Update a todo")
            .WithDescription("Replaces the title and the done flag of a todo.");

        group.MapDelete("/{id:guid}", TodoHandlers.Delete)
            .WithName("DeleteTodo")
            .RequireAuthorization(Policies.ApiWrite)
            .WithSummary("Delete a todo")
            .WithDescription("Deletes a todo. Returns 404 if it does not exist.");

        return app;
    }
}
