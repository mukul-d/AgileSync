using AgileSync.ProjectService.Dtos;
using AgileSync.ProjectService.Models;
using AgileSync.Shared.Filters;
using AgileSync.Shared.Models;
using AgileSync.Shared.Repositories;

namespace AgileSync.ProjectService.Endpoints;

/// <summary>
/// Board CRUD endpoints under <c>/api/projects/boards</c>.
/// Uses C# 14 extension block syntax.
/// </summary>
public static class BoardEndpoints
{
    /// <param name="routes">The endpoint route builder to extend.</param>
    extension(IEndpointRouteBuilder routes)
    {
        /// <summary>Maps all board endpoints.</summary>
        public void MapBoardEndpoints()
        {
            var group = routes.MapGroup("/api/projects/boards").WithTags("Boards");

            group.MapGet("/{projectId}", async (string projectId, IRepository<Board> repo, CancellationToken ct) =>
            {
                var boards = await repo.FindAsync(b => b.ProjectId == projectId, ct);
                return Results.Ok(BaseResponse<IReadOnlyList<Board>>.Ok(boards));
            });

            group.MapPost("/", async (CreateBoardRequest request, IRepository<Board> repo, CancellationToken ct) =>
            {
                var board = new Board
                {
                    ProjectId = request.ProjectId,
                    Name = request.Name
                };
                await repo.CreateAsync(board, ct);
                return Results.Created($"/api/projects/boards/{board.Id}",
                    BaseResponse<Board>.Ok(board, "Board created"));
            })
            .AddEndpointFilter<ValidationFilter<CreateBoardRequest>>();

            group.MapDelete("/{id}", async (string id, IRepository<Board> repo, CancellationToken ct) =>
            {
                await repo.DeleteAsync(id, ct);
                return Results.Ok(BaseResponse.Ok("Board deleted"));
            });
        }
    }
}
