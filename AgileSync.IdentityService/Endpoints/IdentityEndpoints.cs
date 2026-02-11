using System.Collections.Concurrent;
using AgileSync.IdentityService.Dtos;
using AgileSync.IdentityService.Models;
using AgileSync.Shared.Filters;
using AgileSync.Shared.Models;
using AgileSync.Shared.Repositories;

namespace AgileSync.IdentityService.Endpoints;

/// <summary>
/// Identity API endpoints for user registration, authentication, profile, and theme management.
/// Uses C# 14 extension block syntax.
/// </summary>
public static class IdentityEndpoints
{
    private static readonly ConcurrentDictionary<string, (string UserId, DateTime Expiry)> ActiveTokens = new();

    /// <param name="routes">The endpoint route builder to extend.</param>
    extension(IEndpointRouteBuilder routes)
    {
        /// <summary>Maps all identity endpoints under <c>/api/identity</c>.</summary>
        public void MapIdentityEndpoints()
        {
            var group = routes.MapGroup("/api/identity").WithTags("Identity");

            group.MapPost("/register", async (RegisterRequest request, IRepository<User> repo, CancellationToken ct) =>
            {
                var existing = await repo.FindAsync(u => u.Email == request.Email, ct);
                if (existing.Count > 0)
                    return Results.Conflict(BaseResponse.Fail("Email already registered."));

                var user = new User
                {
                    Email = request.Email,
                    DisplayName = request.DisplayName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
                };

                await repo.CreateAsync(user, ct);

                return Results.Created($"/api/identity/users/{user.Id}",
                    BaseResponse<AuthResponse>.Ok(
                        new AuthResponse(user.Id, user.Email, user.DisplayName, user.Role.ToString()),
                        "Registration successful"));
            })
            .AddEndpointFilter<ValidationFilter<RegisterRequest>>();

            group.MapPost("/login", async (LoginRequest request, IRepository<User> repo, CancellationToken ct) =>
            {
                var users = await repo.FindAsync(u => u.Email == request.Email, ct);
                var user = users.FirstOrDefault();

                if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                    return Results.Json(BaseResponse.Fail("Invalid email or password"), statusCode: 401);

                var token = Guid.NewGuid().ToString("N");
                ActiveTokens[token] = (user.Id, DateTime.UtcNow.AddHours(8));

                return Results.Ok(BaseResponse<LoginResponse>.Ok(
                    new LoginResponse(token, user.Id, user.Email, user.DisplayName, user.Role.ToString())));
            })
            .AddEndpointFilter<ValidationFilter<LoginRequest>>();

            group.MapGet("/me", async (HttpContext ctx, IRepository<User> repo, CancellationToken ct) =>
            {
                var userId = GetAuthenticatedUserId(ctx);
                if (userId is null)
                    return Results.Json(BaseResponse.Fail("Unauthorized"), statusCode: 401);

                var user = await repo.GetByIdAsync(userId, ct);
                if (user is null)
                    return Results.Json(BaseResponse.Fail("Unauthorized"), statusCode: 401);

                return Results.Ok(BaseResponse<AuthResponse>.Ok(
                    new AuthResponse(user.Id, user.Email, user.DisplayName, user.Role.ToString())));
            });

            group.MapPost("/logout", (HttpContext ctx) =>
            {
                var authHeader = ctx.Request.Headers.Authorization.ToString();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader["Bearer ".Length..];
                    ActiveTokens.TryRemove(token, out _);
                }
                return Results.Ok(BaseResponse.Ok("Logged out"));
            });

            group.MapGet("/users", async (IRepository<User> repo, CancellationToken ct) =>
            {
                var users = await repo.GetAllAsync(ct);
                var response = users.Select(u => new AuthResponse(
                    u.Id, u.Email, u.DisplayName, u.Role.ToString())).ToList();
                return Results.Ok(BaseResponse<List<AuthResponse>>.Ok(response));
            });

            group.MapGet("/users/{id}", async (string id, IRepository<User> repo, CancellationToken ct) =>
            {
                var user = await repo.GetByIdAsync(id, ct);
                if (user is null)
                    return Results.NotFound(BaseResponse.Fail("User not found"));

                return Results.Ok(BaseResponse<AuthResponse>.Ok(
                    new AuthResponse(user.Id, user.Email, user.DisplayName, user.Role.ToString())));
            });

            group.MapGet("/users/{id}/theme", async (string id, IRepository<User> repo, CancellationToken ct) =>
            {
                var user = await repo.GetByIdAsync(id, ct);
                if (user is null)
                    return Results.NotFound(BaseResponse.Fail("User not found"));

                return Results.Ok(BaseResponse<ThemePreferencesResponse>.Ok(
                    new ThemePreferencesResponse(user.ThemePreferences.Web, user.ThemePreferences.Pwa)));
            });

            group.MapPut("/users/{id}/theme", async (string id, UpdateThemeRequest request, IRepository<User> repo, CancellationToken ct) =>
            {
                var user = await repo.GetByIdAsync(id, ct);
                if (user is null)
                    return Results.NotFound(BaseResponse.Fail("User not found"));

                var theme = request.Theme.ToLowerInvariant();
                if (theme is not ("dark" or "light"))
                    return Results.BadRequest(BaseResponse.Fail("Invalid theme. Valid options: dark, light"));

                var result = request.Platform.ToLowerInvariant() switch
                {
                    "web" => user.ThemePreferences.Web = theme,
                    "pwa" => user.ThemePreferences.Pwa = theme,
                    _ => (string?)null
                };

                if (result is null)
                    return Results.BadRequest(BaseResponse.Fail("Invalid platform. Use 'web' or 'pwa'."));

                await repo.UpdateAsync(user, ct);
                return Results.Ok(BaseResponse<ThemePreferencesResponse>.Ok(
                    new ThemePreferencesResponse(user.ThemePreferences.Web, user.ThemePreferences.Pwa),
                    "Theme updated"));
            })
            .AddEndpointFilter<ValidationFilter<UpdateThemeRequest>>();
        }
    }

    /// <summary>
    /// Creates a user-compatible session token for the superadmin, allowing them to view the main app.
    /// The token is registered in the user token store with a synthetic "superadmin" user ID.
    /// </summary>
    /// <returns>The generated bearer token.</returns>
    internal static string CreateAdminViewToken()
    {
        var token = Guid.NewGuid().ToString("N");
        ActiveTokens[token] = ("superadmin", DateTime.UtcNow.AddHours(8));
        return token;
    }

    /// <summary>Extracts and validates the authenticated user ID from the Bearer token.</summary>
    private static string? GetAuthenticatedUserId(HttpContext ctx)
    {
        var authHeader = ctx.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return null;

        var token = authHeader["Bearer ".Length..];

        if (!ActiveTokens.TryGetValue(token, out var session))
            return null;

        if (DateTime.UtcNow > session.Expiry)
        {
            ActiveTokens.TryRemove(token, out _);
            return null;
        }

        return session.UserId;
    }
}
