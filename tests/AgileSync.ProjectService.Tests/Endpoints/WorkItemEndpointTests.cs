using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AgileSync.ProjectService.Models;
using FluentAssertions;
using NSubstitute;

namespace AgileSync.ProjectService.Tests.Endpoints;

public class WorkItemEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public WorkItemEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListByBoard_ReturnsOk()
    {
        var items = new List<WorkItem>
        {
            new() { ProjectId = "p1", BoardId = "b1", Title = "Item A" },
            new() { ProjectId = "p1", BoardId = "b1", Title = "Item B" }
        };
        _factory.WorkItemRepository.FindAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<WorkItem, bool>>>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<WorkItem>>(items));

        var response = await _client.GetAsync("/api/projects/workitems/b1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("data").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task GetById_Found_ReturnsOk()
    {
        var item = new WorkItem { ProjectId = "p1", BoardId = "b1", Title = "Found" };
        _factory.WorkItemRepository.GetByIdAsync(item.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<WorkItem?>(item));

        var response = await _client.GetAsync($"/api/projects/workitems/item/{item.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        _factory.WorkItemRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<WorkItem?>(null));

        var response = await _client.GetAsync("/api/projects/workitems/item/nonexistent");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateWorkItem_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/projects/workitems", new
        {
            ProjectId = "proj1",
            BoardId = "board1",
            Title = "New Item",
            Description = "desc",
            Type = "Bug",
            Priority = "High",
            AssigneeId = (string?)null,
            SprintId = (string?)null,
            StoryPoints = 5
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateWorkItem_InvalidType_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/projects/workitems", new
        {
            ProjectId = "proj1",
            BoardId = "board1",
            Title = "Bad Type",
            Description = "desc",
            Type = "InvalidType",
            Priority = "High",
            AssigneeId = (string?)null,
            SprintId = (string?)null,
            StoryPoints = (int?)null
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateWorkItem_NotFound_Returns404()
    {
        _factory.WorkItemRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<WorkItem?>(null));

        var response = await _client.PutAsJsonAsync("/api/projects/workitems/nonexistent", new
        {
            Title = "Updated",
            Description = "desc",
            Status = "In Progress",
            Priority = "High",
            AssigneeId = (string?)null,
            SprintId = (string?)null,
            StoryPoints = 3
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteWorkItem_ReturnsOk()
    {
        var response = await _client.DeleteAsync("/api/projects/workitems/any-id");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
