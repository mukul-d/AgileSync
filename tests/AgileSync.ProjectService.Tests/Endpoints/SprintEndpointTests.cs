using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AgileSync.ProjectService.Models;
using FluentAssertions;
using NSubstitute;

namespace AgileSync.ProjectService.Tests.Endpoints;

public class SprintEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public SprintEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListByProject_ReturnsOk()
    {
        var sprints = new List<Sprint>
        {
            new() { ProjectId = "proj1", Name = "Sprint 1" },
            new() { ProjectId = "proj1", Name = "Sprint 2" }
        };
        _factory.SprintRepository.FindAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Sprint, bool>>>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Sprint>>(sprints));

        var response = await _client.GetAsync("/api/projects/sprints/proj1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("data").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task CreateSprint_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/projects/sprints", new
        {
            ProjectId = "proj1",
            Name = "Sprint 1",
            Goal = "Complete features",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(14)
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateSprint_EmptyName_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/projects/sprints", new
        {
            ProjectId = "proj1",
            Name = "",
            Goal = "Goal",
            StartDate = (DateTime?)null,
            EndDate = (DateTime?)null
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteSprint_ReturnsOk()
    {
        var response = await _client.DeleteAsync("/api/projects/sprints/sprint-id");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
