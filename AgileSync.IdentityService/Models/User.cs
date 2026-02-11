using AgileSync.Shared.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace AgileSync.IdentityService.Models;

/// <summary>
/// Represents a user account with authentication data, role, and organization memberships.
/// Email is auto-normalized to lowercase via the C# 14 <c>field</c> keyword.
/// </summary>
public class User : BaseEntity
{
    /// <summary>User's email address, auto-normalized to lowercase and trimmed.</summary>
    [BsonElement("email")]
    public required string Email
    {
        get => field;
        set => field = value.ToLowerInvariant().Trim();
    }

    /// <summary>User's display name shown in the UI.</summary>
    [BsonElement("displayName")]
    public required string DisplayName { get; set; }

    /// <summary>BCrypt-hashed password.</summary>
    [BsonElement("passwordHash")]
    public required string PasswordHash { get; set; }

    /// <summary>Global role of the user (Admin, Member, Viewer).</summary>
    [BsonElement("role")]
    public UserRole Role { get; set; } = UserRole.Member;

    /// <summary>Whether the user account is active.</summary>
    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    /// <summary>Organization memberships with per-org roles.</summary>
    [BsonElement("memberships")]
    public List<OrgMembership> Memberships { get; set; } = [];

    /// <summary>Platform-specific theme preferences.</summary>
    [BsonElement("themePreferences")]
    public ThemePreferences ThemePreferences { get; set; } = new();
}

/// <summary>
/// Theme preferences for web and PWA platforms.
/// Values are self-validated via the <c>field</c> keyword to only accept "dark" or "light".
/// </summary>
public class ThemePreferences
{
    /// <summary>Theme for the web platform ("dark" or "light").</summary>
    [BsonElement("web")]
    public string Web
    {
        get => field;
        set => field = value is "dark" or "light" ? value : "dark";
    } = "dark";

    /// <summary>Theme for the PWA platform ("dark" or "light").</summary>
    [BsonElement("pwa")]
    public string Pwa
    {
        get => field;
        set => field = value is "dark" or "light" ? value : "dark";
    } = "dark";
}

/// <summary>Tracks a user's membership and role within an organization.</summary>
public class OrgMembership
{
    /// <summary>The ID of the organization the user belongs to.</summary>
    [BsonElement("organizationId")]
    public required string OrganizationId { get; set; }

    /// <summary>The user's role within this organization.</summary>
    [BsonElement("role")]
    public OrgRole Role { get; set; } = OrgRole.Member;

    /// <summary>When the user joined this organization.</summary>
    [BsonElement("joinedAt")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Global user roles.</summary>
public enum UserRole
{
    /// <summary>Full admin access.</summary>
    Admin,
    /// <summary>Standard member access.</summary>
    Member,
    /// <summary>Read-only access.</summary>
    Viewer
}

/// <summary>Per-organization user roles.</summary>
public enum OrgRole
{
    /// <summary>Organization owner with full control.</summary>
    Owner,
    /// <summary>Organization administrator.</summary>
    Admin,
    /// <summary>Standard organization member.</summary>
    Member,
    /// <summary>Read-only organization access.</summary>
    Viewer
}
