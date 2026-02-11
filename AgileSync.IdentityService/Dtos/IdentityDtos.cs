namespace AgileSync.IdentityService.Dtos;

// ── Auth ────────────────────────────────────────────────

/// <summary>Request to register a new user account.</summary>
public record RegisterRequest(string Email, string DisplayName, string Password);

/// <summary>Request to authenticate a user by email and password.</summary>
public record LoginRequest(string Email, string Password);

/// <summary>Public-facing user information (no sensitive data).</summary>
public record AuthResponse(string Id, string Email, string DisplayName, string Role);

/// <summary>Successful login response containing a bearer token and user info.</summary>
public record LoginResponse(string Token, string Id, string Email, string DisplayName, string Role);

// ── Super Admin ─────────────────────────────────────────

/// <summary>Request to authenticate the super admin.</summary>
public record AdminLoginRequest(string Username, string Password);

/// <summary>Successful super-admin login response.</summary>
public record AdminLoginResponse(string Token);

// ── Organizations ───────────────────────────────────────

/// <summary>Request to create a new tenant organization.</summary>
public record CreateOrganizationRequest(string Name, string Slug, string Description);

/// <summary>Request to update an existing organization.</summary>
public record UpdateOrganizationRequest(string Name, string Description, bool IsActive);

/// <summary>Public-facing organization data returned by the API.</summary>
public record OrganizationResponse(string Id, string Name, string Slug, string Description, bool IsActive, DateTime CreatedAt);

// ── Tenant Admins ───────────────────────────────────────

/// <summary>Request to add an administrator to a tenant organization.</summary>
public record AddTenantAdminRequest(string Email, string DisplayName, string Password);

/// <summary>Tenant admin information returned by the API.</summary>
public record TenantAdminResponse(string Id, string Email, string DisplayName, string Role, DateTime JoinedAt);

// ── Theme ───────────────────────────────────────────────

/// <summary>Request to update a user's theme preference for a specific platform.</summary>
public record UpdateThemeRequest(string Platform, string Theme);

/// <summary>Current theme preferences for web and PWA platforms.</summary>
public record ThemePreferencesResponse(string Web, string Pwa);
