using System.Collections.Concurrent;
using AgileSync.IdentityService.Dtos;
using AgileSync.IdentityService.Models;
using AgileSync.Shared.Filters;
using AgileSync.Shared.Models;
using AgileSync.Shared.Repositories;

namespace AgileSync.IdentityService.Endpoints;

/// <summary>
/// Admin API endpoints for superadmin authentication and tenant management.
/// Uses C# 14 extension block syntax.
/// </summary>
public static class AdminEndpoints
{
    private static readonly ConcurrentDictionary<string, DateTime> ActiveTokens = new();

    /// <param name="routes">The endpoint route builder to extend.</param>
    extension(IEndpointRouteBuilder routes)
    {
        /// <summary>Maps all admin endpoints under <c>/api/identity/admin</c>.</summary>
        public void MapAdminEndpoints()
        {
            var group = routes.MapGroup("/api/identity/admin").WithTags("Admin");

            group.MapPost("/login", (AdminLoginRequest request, IConfiguration config, ILogger<Program> logger) =>
            {
                var expectedUsername = config["SuperAdmin:Username"];
                var expectedPassword = config["SuperAdmin:Password"];

                logger.LogInformation("Admin login attempt for user '{Username}'. Config loaded: Username={HasUser}, Password={HasPass}",
                    request.Username,
                    !string.IsNullOrEmpty(expectedUsername),
                    !string.IsNullOrEmpty(expectedPassword));

                if (string.IsNullOrEmpty(expectedUsername) || string.IsNullOrEmpty(expectedPassword))
                {
                    logger.LogError("SuperAdmin config is missing! Check appsettings.json for SuperAdmin:Username and SuperAdmin:Password.");
                    return Results.Json(BaseResponse.Fail("Server misconfiguration"), statusCode: 500);
                }

                if (request.Username != expectedUsername || request.Password != expectedPassword)
                    return Results.Json(BaseResponse.Fail("Invalid credentials"), statusCode: 401);

                var token = Guid.NewGuid().ToString("N");
                ActiveTokens[token] = DateTime.UtcNow.AddHours(8);

                logger.LogInformation("Admin login successful for '{Username}'", request.Username);
                return Results.Ok(BaseResponse<AdminLoginResponse>.Ok(new AdminLoginResponse(token)));
            })
            .AddEndpointFilter<ValidationFilter<AdminLoginRequest>>();

            group.MapGet("/organizations", async (HttpContext ctx, IRepository<Organization> repo, CancellationToken ct) =>
            {
                if (!IsAuthorized(ctx))
                    return Results.Json(BaseResponse.Fail("Unauthorized"), statusCode: 401);

                var orgs = await repo.GetAllAsync(ct);
                var response = orgs.Select(o => ToResponse(o)).ToList();
                return Results.Ok(BaseResponse<List<OrganizationResponse>>.Ok(response));
            });

            group.MapGet("/organizations/{id}", async (string id, HttpContext ctx, IRepository<Organization> repo, CancellationToken ct) =>
            {
                if (!IsAuthorized(ctx))
                    return Results.Json(BaseResponse.Fail("Unauthorized"), statusCode: 401);

                var org = await repo.GetByIdAsync(id, ct);
                if (org is null)
                    return Results.NotFound(BaseResponse.Fail("Organization not found"));

                return Results.Ok(BaseResponse<OrganizationResponse>.Ok(ToResponse(org)));
            });

            group.MapPost("/organizations", async (CreateOrganizationRequest request, HttpContext ctx, IRepository<Organization> repo, CancellationToken ct) =>
            {
                if (!IsAuthorized(ctx))
                    return Results.Json(BaseResponse.Fail("Unauthorized"), statusCode: 401);

                var existing = await repo.FindAsync(o => o.Slug == request.Slug, ct);
                if (existing.Count > 0)
                    return Results.Conflict(BaseResponse.Fail("An organization with this slug already exists."));

                var org = new Organization
                {
                    Name = request.Name,
                    Slug = request.Slug,
                    Description = request.Description
                };
                org.TenantId = org.Id;

                await repo.CreateAsync(org, ct);
                return Results.Created($"/api/identity/admin/organizations/{org.Id}",
                    BaseResponse<OrganizationResponse>.Ok(ToResponse(org), "Organization created"));
            })
            .AddEndpointFilter<ValidationFilter<CreateOrganizationRequest>>();

            group.MapPut("/organizations/{id}", async (string id, UpdateOrganizationRequest request, HttpContext ctx, IRepository<Organization> repo, CancellationToken ct) =>
            {
                if (!IsAuthorized(ctx))
                    return Results.Json(BaseResponse.Fail("Unauthorized"), statusCode: 401);

                var org = await repo.GetByIdAsync(id, ct);
                if (org is null)
                    return Results.NotFound(BaseResponse.Fail("Organization not found"));

                org.Name = request.Name;
                org.Description = request.Description;
                org.IsActive = request.IsActive;

                await repo.UpdateAsync(org, ct);
                return Results.Ok(BaseResponse<OrganizationResponse>.Ok(ToResponse(org), "Organization updated"));
            })
            .AddEndpointFilter<ValidationFilter<UpdateOrganizationRequest>>();

            group.MapDelete("/organizations/{id}", async (string id, HttpContext ctx, IRepository<Organization> repo, CancellationToken ct) =>
            {
                if (!IsAuthorized(ctx))
                    return Results.Json(BaseResponse.Fail("Unauthorized"), statusCode: 401);

                var org = await repo.GetByIdAsync(id, ct);
                if (org is null)
                    return Results.NotFound(BaseResponse.Fail("Organization not found"));

                org.IsActive = false;
                await repo.UpdateAsync(org, ct);
                return Results.Ok(BaseResponse.Ok("Organization deactivated"));
            });

            // ── Admin app-view token ──

            group.MapPost("/app-token", (HttpContext ctx) =>
            {
                if (!IsAuthorized(ctx))
                    return Results.Json(BaseResponse.Fail("Unauthorized"), statusCode: 401);

                var token = IdentityEndpoints.CreateAdminViewToken();
                return Results.Ok(BaseResponse<LoginResponse>.Ok(
                    new LoginResponse(token, "superadmin", "admin@agilesync.local", "Super Admin", "SuperAdmin")));
            });

            // ── Tenant Admin management ──

            group.MapGet("/organizations/{orgId}/admins", async (string orgId, HttpContext ctx, IRepository<Organization> orgRepo, IRepository<User> userRepo, CancellationToken ct) =>
            {
                if (!IsAuthorized(ctx))
                    return Results.Json(BaseResponse.Fail("Unauthorized"), statusCode: 401);

                var org = await orgRepo.GetByIdAsync(orgId, ct);
                if (org is null)
                    return Results.NotFound(BaseResponse.Fail("Organization not found"));

                var users = await userRepo.FindAsync(u => u.Memberships.Any(m => m.OrganizationId == orgId), ct);
                var response = users.Select(u =>
                {
                    var membership = u.Memberships.First(m => m.OrganizationId == orgId);
                    return new TenantAdminResponse(u.Id, u.Email, u.DisplayName, membership.Role.ToString(), membership.JoinedAt);
                }).ToList();

                return Results.Ok(BaseResponse<List<TenantAdminResponse>>.Ok(response));
            });

            group.MapPost("/organizations/{orgId}/admins", async (string orgId, AddTenantAdminRequest request, HttpContext ctx, IRepository<Organization> orgRepo, IRepository<User> userRepo, CancellationToken ct) =>
            {
                if (!IsAuthorized(ctx))
                    return Results.Json(BaseResponse.Fail("Unauthorized"), statusCode: 401);

                var org = await orgRepo.GetByIdAsync(orgId, ct);
                if (org is null)
                    return Results.NotFound(BaseResponse.Fail("Organization not found"));

                var existing = await userRepo.FindAsync(u => u.Email == request.Email, ct);
                var user = existing.FirstOrDefault();

                if (user is not null)
                {
                    if (user.Memberships.Any(m => m.OrganizationId == orgId))
                        return Results.Conflict(BaseResponse.Fail("User is already a member of this organization."));

                    user.Memberships.Add(new OrgMembership
                    {
                        OrganizationId = orgId,
                        Role = OrgRole.Admin
                    });
                    user.TenantId ??= orgId;
                    await userRepo.UpdateAsync(user, ct);
                }
                else
                {
                    user = new User
                    {
                        Email = request.Email,
                        DisplayName = request.DisplayName,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                        Role = UserRole.Admin,
                        TenantId = orgId,
                        Memberships =
                        [
                            new OrgMembership
                            {
                                OrganizationId = orgId,
                                Role = OrgRole.Admin
                            }
                        ]
                    };
                    await userRepo.CreateAsync(user, ct);
                }

                var m = user.Memberships.First(m => m.OrganizationId == orgId);
                return Results.Created(
                    $"/api/identity/admin/organizations/{orgId}/admins/{user.Id}",
                    BaseResponse<TenantAdminResponse>.Ok(
                        new TenantAdminResponse(user.Id, user.Email, user.DisplayName, m.Role.ToString(), m.JoinedAt),
                        "Admin added"));
            })
            .AddEndpointFilter<ValidationFilter<AddTenantAdminRequest>>();

            group.MapDelete("/organizations/{orgId}/admins/{userId}", async (string orgId, string userId, HttpContext ctx, IRepository<User> userRepo, CancellationToken ct) =>
            {
                if (!IsAuthorized(ctx))
                    return Results.Json(BaseResponse.Fail("Unauthorized"), statusCode: 401);

                var user = await userRepo.GetByIdAsync(userId, ct);
                if (user is null)
                    return Results.NotFound(BaseResponse.Fail("User not found"));

                var membership = user.Memberships.FirstOrDefault(m => m.OrganizationId == orgId);
                if (membership is null)
                    return Results.NotFound(BaseResponse.Fail("User is not a member of this organization"));

                user.Memberships.Remove(membership);
                await userRepo.UpdateAsync(user, ct);
                return Results.Ok(BaseResponse.Ok("Admin removed"));
            });
        }
    }

    /// <summary>Validates the Bearer token in the Authorization header against active tokens.</summary>
    private static bool IsAuthorized(HttpContext ctx)
    {
        var authHeader = ctx.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return false;

        var token = authHeader["Bearer ".Length..];

        if (!ActiveTokens.TryGetValue(token, out var expiry))
            return false;

        if (DateTime.UtcNow > expiry)
        {
            ActiveTokens.TryRemove(token, out _);
            return false;
        }

        return true;
    }

    /// <summary>Maps an <see cref="Organization"/> entity to its API response DTO.</summary>
    private static OrganizationResponse ToResponse(Organization org) =>
        new(org.Id, org.Name, org.Slug, org.Description, org.IsActive, org.CreatedAt);
}
