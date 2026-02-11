using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AgileSync.ProjectService.Models;
using FluentAssertions;
using NSubstitute;

namespace AgileSync.ProjectService.Tests.Endpoints;

public class BoardEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public BoardEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListBoards_ByProject_ReturnsOk()
    {
        var boards = new List<Board>
        {
            new() { ProjectId = "proj1", Name = "Board A" },
            new() { ProjectId = "proj1", Name = "Board B" }
        };
        _factory.BoardRepository.FindAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Board, bool>>>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Board>>(boards));

        var response = await _client.GetAsync("/api/projects/boards/proj1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("data").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task CreateBoard_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/projects/boards", new
        {
            ProjectId = "proj1",
            Name = "New Board"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateBoard_EmptyName_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/projects/boards", new
        {
            ProjectId = "proj1",
            Name = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteBoard_ReturnsOk()
    {
        var response = await _client.DeleteAsync("/api/projects/boards/board-id");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
