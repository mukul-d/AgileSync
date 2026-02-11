using AgileSync.Shared.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace AgileSync.ProjectService.Models;

/// <summary>
/// Represents a work item (task, story, bug, or epic) on a board within a project.
/// </summary>
public class WorkItem : BaseEntity
{
    /// <summary>ID of the project this work item belongs to.</summary>
    [BsonElement("projectId")]
    public required string ProjectId { get; set; }

    /// <summary>ID of the board this work item is on.</summary>
    [BsonElement("boardId")]
    public required string BoardId { get; set; }

    /// <summary>Short title/summary of the work item.</summary>
    [BsonElement("title")]
    public required string Title { get; set; }

    /// <summary>Detailed description of what needs to be done.</summary>
    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>The category of work item (Epic, Story, Task, Bug).</summary>
    [BsonElement("type")]
    public WorkItemType Type { get; set; } = WorkItemType.Task;

    /// <summary>Current workflow status (e.g., "To Do", "In Progress", "Done").</summary>
    [BsonElement("status")]
    public string Status { get; set; } = "To Do";

    /// <summary>Priority level of the work item.</summary>
    [BsonElement("priority")]
    public Priority Priority { get; set; } = Priority.Medium;

    /// <summary>ID of the user assigned to this work item (null if unassigned).</summary>
    [BsonElement("assigneeId")]
    public string? AssigneeId { get; set; }

    /// <summary>ID of the sprint this work item is scheduled in (null if in backlog).</summary>
    [BsonElement("sprintId")]
    public string? SprintId { get; set; }

    /// <summary>Estimated effort in story points.</summary>
    [BsonElement("storyPoints")]
    public int? StoryPoints { get; set; }

    /// <summary>Free-form tags for categorization and filtering.</summary>
    [BsonElement("tags")]
    public List<string> Tags { get; set; } = [];
}

/// <summary>Categories of work items in the agile workflow.</summary>
public enum WorkItemType
{
    /// <summary>Large body of work spanning multiple sprints.</summary>
    Epic,
    /// <summary>User-facing feature or requirement.</summary>
    Story,
    /// <summary>Technical or implementation task.</summary>
    Task,
    /// <summary>Defect or issue to be fixed.</summary>
    Bug
}

/// <summary>Priority levels for work items, ordered from most to least urgent.</summary>
public enum Priority
{
    /// <summary>Blocking issue requiring immediate attention.</summary>
    Critical,
    /// <summary>Important item to address soon.</summary>
    High,
    /// <summary>Normal priority.</summary>
    Medium,
    /// <summary>Nice-to-have, address when time permits.</summary>
    Low
}
