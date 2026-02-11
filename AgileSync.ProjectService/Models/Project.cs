using AgileSync.Shared.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace AgileSync.ProjectService.Models;

/// <summary>
/// Represents an agile project with a unique key, owner, and team members.
/// </summary>
public class Project : BaseEntity
{
    /// <summary>Display name of the project.</summary>
    [BsonElement("name")]
    public required string Name { get; set; }

    /// <summary>Detailed description of the project.</summary>
    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Short uppercase key used as a prefix for work items (e.g., "PROJ").</summary>
    [BsonElement("key")]
    public required string Key { get; set; }

    /// <summary>ID of the user who owns the project.</summary>
    [BsonElement("ownerId")]
    public required string OwnerId { get; set; }

    /// <summary>IDs of users who are members of this project.</summary>
    [BsonElement("memberIds")]
    public List<string> MemberIds { get; set; } = [];

    /// <summary>Current lifecycle status of the project.</summary>
    [BsonElement("status")]
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
}

/// <summary>Lifecycle status of a project.</summary>
public enum ProjectStatus
{
    /// <summary>Project is actively being worked on.</summary>
    Active,
    /// <summary>Project has been archived.</summary>
    Archived,
    /// <summary>Project has been soft-deleted.</summary>
    Deleted
}
