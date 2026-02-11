using AgileSync.ProjectService.Dtos;
using AgileSync.ProjectService.Models;
using AgileSync.Shared.Filters;
using AgileSync.Shared.Models;
using AgileSync.Shared.Repositories;

namespace AgileSync.ProjectService.Endpoints;

/// <summary>
/// Sprint CRUD endpoints under <c>/api/projects/sprints</c>.
/// Uses C# 14 extension block syntax.
/// </summary>
public static class SprintEndpoints
{
    /// <param name="routes">The endpoint route builder to extend.</param>
    extension(IEndpointRouteBuilder routes)
    {
        /// <summary>Maps all sprint endpoints.</summary>
        public void MapSprintEndpoints()
        {
            var group = routes.MapGroup("/api/projects/sprints").WithTags("Sprints");

            group.MapGet("/{projectId}", async (string projectId, IRepository<Sprint> repo, CancellationToken ct) =>
            {
                var sprints = await repo.FindAsync(s => s.ProjectId == projectId, ct);
                return Results.Ok(BaseResponse<IReadOnlyList<Sprint>>.Ok(sprints));
            });

            group.MapPost("/", async (CreateSprintRequest request, IRepository<Sprint> repo, CancellationToken ct) =>
            {
                var sprint = new Sprint
                {
                    ProjectId = request.ProjectId,
                    Name = request.Name,
                    Goal = request.Goal,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                };
                await repo.CreateAsync(sprint, ct);
                return Results.Created($"/api/projects/sprints/{sprint.Id}",
                    BaseResponse<Sprint>.Ok(sprint, "Sprint created"));
            })
            .AddEndpointFilter<ValidationFilter<CreateSprintRequest>>();

            group.MapDelete("/{id}", async (string id, IRepository<Sprint> repo, CancellationToken ct) =>
            {
                await repo.DeleteAsync(id, ct);
                return Results.Ok(BaseResponse.Ok("Sprint deleted"));
            });
        }
    }
}
