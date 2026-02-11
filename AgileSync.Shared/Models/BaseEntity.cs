using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AgileSync.Shared.Models;

/// <summary>
/// Abstract base class for all MongoDB-persisted entities.
/// Provides common fields: Id, TenantId, and audit timestamps.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Unique identifier, auto-generated as a MongoDB ObjectId.</summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    /// <summary>The tenant (organization) this entity belongs to.</summary>
    [BsonElement("tenantId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? TenantId { get; set; }

    /// <summary>UTC timestamp of when this entity was created.</summary>
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of when this entity was last updated.</summary>
    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
