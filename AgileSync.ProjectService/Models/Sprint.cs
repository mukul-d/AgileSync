using AgileSync.Shared.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace AgileSync.ProjectService.Models;

/// <summary>
/// Represents a time-boxed sprint within a project.
/// </summary>
public class Sprint : BaseEntity
{
    /// <summary>ID of the project this sprint belongs to.</summary>
    [BsonElement("projectId")]
    public required string ProjectId { get; set; }

    /// <summary>Display name of the sprint (e.g., "Sprint 12").</summary>
    [BsonElement("name")]
    public required string Name { get; set; }

    /// <summary>The sprint goal describing what the team aims to achieve.</summary>
    [BsonElement("goal")]
    public string Goal { get; set; } = string.Empty;

    /// <summary>Planned start date of the sprint.</summary>
    [BsonElement("startDate")]
    public DateTime? StartDate { get; set; }

    /// <summary>Planned end date of the sprint.</summary>
    [BsonElement("endDate")]
    public DateTime? EndDate { get; set; }

    /// <summary>Current lifecycle status of the sprint.</summary>
    [BsonElement("status")]
    public SprintStatus Status { get; set; } = SprintStatus.Planning;
}

/// <summary>Lifecycle status of a sprint.</summary>
public enum SprintStatus
{
    /// <summary>Sprint is being planned and not yet started.</summary>
    Planning,
    /// <summary>Sprint is currently in progress.</summary>
    Active,
    /// <summary>Sprint has been completed.</summary>
    Completed,
    /// <summary>Sprint was cancelled before completion.</summary>
    Cancelled
}
