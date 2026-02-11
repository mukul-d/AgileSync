using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AgileSync.ProjectService.Models;
using FluentAssertions;
using NSubstitute;

namespace AgileSync.ProjectService.Tests.Endpoints;

public class ProjectEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ProjectEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListProjects_ReturnsOk()
    {
        var projects = new List<Project>
        {
            new() { Name = "Project A", Key = "PA", OwnerId = "owner1" },
            new() { Name = "Project B", Key = "PB", OwnerId = "owner2" }
        };
        _factory.ProjectRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Project>>(projects));

        var response = await _client.GetAsync("/api/projects");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task GetById_Found_ReturnsOk()
    {
        var project = new Project { Name = "Found", Key = "FND", OwnerId = "owner" };
        _factory.ProjectRepository.GetByIdAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Project?>(project));

        var response = await _client.GetAsync($"/api/projects/{project.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        _factory.ProjectRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Project?>(null));

        var response = await _client.GetAsync("/api/projects/nonexistent");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProject_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/projects", new
        {
            Name = "New Project",
            Description = "desc",
            Key = "NP",
            OwnerId = "owner1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task CreateProject_InvalidKey_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/projects", new
        {
            Name = "Bad Key",
            Description = "desc",
            Key = "lowercase",
            OwnerId = "owner1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProject_NotFound_Returns404()
    {
        _factory.ProjectRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Project?>(null));

        var response = await _client.PutAsJsonAsync("/api/projects/nonexistent", new
        {
            Name = "Updated",
            Description = "desc"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProject_ReturnsOk()
    {
        var response = await _client.DeleteAsync("/api/projects/any-id");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
