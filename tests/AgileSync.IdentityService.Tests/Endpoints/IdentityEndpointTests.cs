using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AgileSync.IdentityService.Models;
using AgileSync.Shared.Models;
using FluentAssertions;
using NSubstitute;

namespace AgileSync.IdentityService.Tests.Endpoints;

public class IdentityEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public IdentityEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Success_ReturnsCreated()
    {
        _factory.UserRepository.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<User>>([]));

        var response = await _client.PostAsJsonAsync("/api/identity/register", new
        {
            Email = "new@example.com",
            DisplayName = "New User",
            Password = "password123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").GetProperty("email").GetString().Should().Be("new@example.com");
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var existingUser = new User
        {
            Email = "existing@example.com",
            DisplayName = "Existing",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
        };

        _factory.UserRepository.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<User>>([existingUser]));

        var response = await _client.PostAsJsonAsync("/api/identity/register", new
        {
            Email = "existing@example.com",
            DisplayName = "Another",
            Password = "password123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_Success_ReturnsToken()
    {
        var user = new User
        {
            Email = "login@example.com",
            DisplayName = "Login User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
        };

        _factory.UserRepository.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<User>>([user]));

        var response = await _client.PostAsJsonAsync("/api/identity/login", new
        {
            Email = "login@example.com",
            Password = "correctpassword"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var user = new User
        {
            Email = "login@example.com",
            DisplayName = "Login User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
        };

        _factory.UserRepository.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<User>>([user]));

        var response = await _client.PostAsJsonAsync("/api/identity/login", new
        {
            Email = "login@example.com",
            Password = "wrongpassword"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/identity/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ReturnsOk()
    {
        var response = await _client.PostAsync("/api/identity/logout", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUsers_ReturnsList()
    {
        var users = new List<User>
        {
            new() { Email = "a@test.com", DisplayName = "A", PasswordHash = "hash" },
            new() { Email = "b@test.com", DisplayName = "B", PasswordHash = "hash" }
        };

        _factory.UserRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<User>>(users));

        var response = await _client.GetAsync("/api/identity/users");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("data").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task GetUserById_NotFound_Returns404()
    {
        _factory.UserRepository.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        var response = await _client.GetAsync("/api/identity/users/nonexistent");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Register_InvalidEmail_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/identity/register", new
        {
            Email = "not-an-email",
            DisplayName = "Test",
            Password = "password123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("success").GetBoolean().Should().BeFalse();
    }
}
