using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AgileSync.IdentityService.Models;
using FluentAssertions;
using NSubstitute;

namespace AgileSync.IdentityService.Tests.Endpoints;

public class AdminEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AdminEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> LoginAsAdmin()
    {
        var response = await _client.PostAsJsonAsync("/api/identity/admin/login", new
        {
            Username = "testadmin",
            Password = "testpass"
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        return body.GetProperty("data").GetProperty("token").GetString()!;
    }

    private void SetAuthHeader(string token) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    [Fact]
    public async Task AdminLogin_Success_ReturnsToken()
    {
        var response = await _client.PostAsJsonAsync("/api/identity/admin/login", new
        {
            Username = "testadmin",
            Password = "testpass"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task AdminLogin_WrongCreds_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/identity/admin/login", new
        {
            Username = "wrong",
            Password = "wrong"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOrganizations_Unauthorized_Returns401()
    {
        var response = await _client.GetAsync("/api/identity/admin/organizations");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOrganizations_Authorized_ReturnsList()
    {
        var token = await LoginAsAdmin();
        SetAuthHeader(token);

        var orgs = new List<Organization>
        {
            new() { Name = "Org A", Slug = "org-a" },
            new() { Name = "Org B", Slug = "org-b" }
        };
        _factory.OrgRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Organization>>(orgs));

        var response = await _client.GetAsync("/api/identity/admin/organizations");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("data").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task CreateOrganization_Success_ReturnsCreated()
    {
        var token = await LoginAsAdmin();
        SetAuthHeader(token);

        _factory.OrgRepository.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Organization, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Organization>>([]));

        var response = await _client.PostAsJsonAsync("/api/identity/admin/organizations", new
        {
            Name = "New Org",
            Slug = "new-org",
            Description = "A new organization"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").GetProperty("slug").GetString().Should().Be("new-org");
    }

    [Fact]
    public async Task CreateOrganization_DuplicateSlug_ReturnsConflict()
    {
        var token = await LoginAsAdmin();
        SetAuthHeader(token);

        var existing = new Organization { Name = "Existing", Slug = "dup-slug" };
        _factory.OrgRepository.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Organization, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Organization>>([existing]));

        var response = await _client.PostAsJsonAsync("/api/identity/admin/organizations", new
        {
            Name = "Another Org",
            Slug = "dup-slug",
            Description = "dup"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateOrganization_NotFound_Returns404()
    {
        var token = await LoginAsAdmin();
        SetAuthHeader(token);

        _factory.OrgRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Organization?>(null));

        var response = await _client.PutAsJsonAsync("/api/identity/admin/organizations/nonexistent", new
        {
            Name = "Updated",
            Description = "desc",
            IsActive = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateOrganization_Success_ReturnsOk()
    {
        var token = await LoginAsAdmin();
        SetAuthHeader(token);

        var org = new Organization { Name = "To Deactivate", Slug = "deactivate" };
        _factory.OrgRepository.GetByIdAsync(org.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Organization?>(org));

        var response = await _client.DeleteAsync($"/api/identity/admin/organizations/{org.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task AppToken_Authorized_ReturnsToken()
    {
        var token = await LoginAsAdmin();
        SetAuthHeader(token);

        var response = await _client.PostAsync("/api/identity/admin/app-token", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("data").GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task AppToken_Unauthorized_Returns401()
    {
        var response = await _client.PostAsync("/api/identity/admin/app-token", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddTenantAdmin_OrgNotFound_Returns404()
    {
        var token = await LoginAsAdmin();
        SetAuthHeader(token);

        _factory.OrgRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Organization?>(null));

        var response = await _client.PostAsJsonAsync("/api/identity/admin/organizations/nonexistent/admins", new
        {
            Email = "admin@org.com",
            DisplayName = "Admin",
            Password = "password123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveTenantAdmin_UserNotFound_Returns404()
    {
        var token = await LoginAsAdmin();
        SetAuthHeader(token);

        _factory.UserRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        var response = await _client.DeleteAsync("/api/identity/admin/organizations/org1/admins/user1");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
