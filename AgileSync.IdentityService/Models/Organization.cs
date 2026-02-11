using AgileSync.Shared.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace AgileSync.IdentityService.Models;

/// <summary>
/// Represents a tenant organization in the multi-tenant system.
/// Each organization has a unique slug and can be activated/deactivated.
/// </summary>
public class Organization : BaseEntity
{
    /// <summary>Display name of the organization.</summary>
    [BsonElement("name")]
    public required string Name { get; set; }

    /// <summary>
    /// URL-friendly identifier, auto-normalized to lowercase with trimmed whitespace
    /// via the C# 14 <c>field</c> keyword.
    /// </summary>
    [BsonElement("slug")]
    public required string Slug
    {
        get => field;
        set => field = value.ToLowerInvariant().Trim();
    }

    /// <summary>Optional description of the organization.</summary>
    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Whether the organization is currently active.</summary>
    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
}
