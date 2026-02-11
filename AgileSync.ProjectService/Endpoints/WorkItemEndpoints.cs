using AgileSync.ProjectService.Dtos;
using AgileSync.ProjectService.Models;
using AgileSync.Shared.Filters;
using AgileSync.Shared.Models;
using AgileSync.Shared.Repositories;

namespace AgileSync.ProjectService.Endpoints;

/// <summary>
/// Work item CRUD endpoints under <c>/api/projects/workitems</c>.
/// Uses C# 14 extension block syntax.
/// </summary>
public static class WorkItemEndpoints
{
    /// <param name="routes">The endpoint route builder to extend.</param>
    extension(IEndpointRouteBuilder routes)
    {
        /// <summary>Maps all work item endpoints.</summary>
        public void MapWorkItemEndpoints()
        {
            var group = routes.MapGroup("/api/projects/workitems").WithTags("WorkItems");

            group.MapGet("/{boardId}", async (string boardId, IRepository<WorkItem> repo, CancellationToken ct) =>
            {
                var items = await repo.FindAsync(w => w.BoardId == boardId, ct);
                return Results.Ok(BaseResponse<IReadOnlyList<WorkItem>>.Ok(items));
            });

            group.MapGet("/item/{id}", async (string id, IRepository<WorkItem> repo, CancellationToken ct) =>
            {
                var item = await repo.GetByIdAsync(id, ct);
                if (item is null)
                    return Results.NotFound(BaseResponse.Fail("Work item not found"));

                return Results.Ok(BaseResponse<WorkItem>.Ok(item));
            });

            group.MapPost("/", async (CreateWorkItemRequest request, IRepository<WorkItem> repo, CancellationToken ct) =>
            {
                var item = new WorkItem
                {
                    ProjectId = request.ProjectId,
                    BoardId = request.BoardId,
                    Title = request.Title,
                    Description = request.Description,
                    Type = Enum.Parse<WorkItemType>(request.Type, ignoreCase: true),
                    Priority = Enum.Parse<Priority>(request.Priority, ignoreCase: true),
                    AssigneeId = request.AssigneeId,
                    SprintId = request.SprintId,
                    StoryPoints = request.StoryPoints
                };
                await repo.CreateAsync(item, ct);
                return Results.Created($"/api/projects/workitems/item/{item.Id}",
                    BaseResponse<WorkItem>.Ok(item, "Work item created"));
            })
            .AddEndpointFilter<ValidationFilter<CreateWorkItemRequest>>();

            group.MapPut("/{id}", async (string id, UpdateWorkItemRequest request, IRepository<WorkItem> repo, CancellationToken ct) =>
            {
                var item = await repo.GetByIdAsync(id, ct);
                if (item is null)
                    return Results.NotFound(BaseResponse.Fail("Work item not found"));

                item.Title = request.Title;
                item.Description = request.Description;
                item.Status = request.Status;
                item.Priority = Enum.Parse<Priority>(request.Priority, ignoreCase: true);
                item.AssigneeId = request.AssigneeId;
                item.SprintId = request.SprintId;
                item.StoryPoints = request.StoryPoints;
                await repo.UpdateAsync(item, ct);
                return Results.Ok(BaseResponse<WorkItem>.Ok(item, "Work item updated"));
            })
            .AddEndpointFilter<ValidationFilter<UpdateWorkItemRequest>>();

            group.MapDelete("/{id}", async (string id, IRepository<WorkItem> repo, CancellationToken ct) =>
            {
                await repo.DeleteAsync(id, ct);
                return Results.Ok(BaseResponse.Ok("Work item deleted"));
            });
        }
    }
}
