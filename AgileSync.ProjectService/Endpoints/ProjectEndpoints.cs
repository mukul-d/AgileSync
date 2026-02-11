using AgileSync.ProjectService.Dtos;
using AgileSync.ProjectService.Models;
using AgileSync.Shared.Filters;
using AgileSync.Shared.Models;
using AgileSync.Shared.Repositories;

namespace AgileSync.ProjectService.Endpoints;

/// <summary>
/// Project CRUD endpoints under <c>/api/projects</c>.
/// Uses C# 14 extension block syntax.
/// </summary>
public static class ProjectEndpoints
{
    /// <param name="routes">The endpoint route builder to extend.</param>
    extension(IEndpointRouteBuilder routes)
    {
        /// <summary>Maps all project endpoints.</summary>
        public void MapProjectEndpoints()
        {
            var group = routes.MapGroup("/api/projects").WithTags("Projects");

            group.MapGet("/", async (IRepository<Project> repo, CancellationToken ct) =>
            {
                var projects = await repo.GetAllAsync(ct);
                return Results.Ok(BaseResponse<IReadOnlyList<Project>>.Ok(projects));
            });

            group.MapGet("/{id}", async (string id, IRepository<Project> repo, CancellationToken ct) =>
            {
                var project = await repo.GetByIdAsync(id, ct);
                if (project is null)
                    return Results.NotFound(BaseResponse.Fail("Project not found"));

                return Results.Ok(BaseResponse<Project>.Ok(project));
            });

            group.MapPost("/", async (CreateProjectRequest request, IRepository<Project> repo, CancellationToken ct) =>
            {
                var project = new Project
                {
                    Name = request.Name,
                    Description = request.Description,
                    Key = request.Key,
                    OwnerId = request.OwnerId
                };
                await repo.CreateAsync(project, ct);
                return Results.Created($"/api/projects/{project.Id}",
                    BaseResponse<Project>.Ok(project, "Project created"));
            })
            .AddEndpointFilter<ValidationFilter<CreateProjectRequest>>();

            group.MapPut("/{id}", async (string id, UpdateProjectRequest request, IRepository<Project> repo, CancellationToken ct) =>
            {
                var project = await repo.GetByIdAsync(id, ct);
                if (project is null)
                    return Results.NotFound(BaseResponse.Fail("Project not found"));

                project.Name = request.Name;
                project.Description = request.Description;
                await repo.UpdateAsync(project, ct);
                return Results.Ok(BaseResponse<Project>.Ok(project, "Project updated"));
            })
            .AddEndpointFilter<ValidationFilter<UpdateProjectRequest>>();

            group.MapDelete("/{id}", async (string id, IRepository<Project> repo, CancellationToken ct) =>
            {
                await repo.DeleteAsync(id, ct);
                return Results.Ok(BaseResponse.Ok("Project deleted"));
            });
        }
    }
}
