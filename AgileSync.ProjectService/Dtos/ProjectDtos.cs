namespace AgileSync.ProjectService.Dtos;

/// <summary>Request to create a new project.</summary>
public record CreateProjectRequest(string Name, string Description, string Key, string OwnerId);

/// <summary>Request to update an existing project's name and description.</summary>
public record UpdateProjectRequest(string Name, string Description);

/// <summary>Request to create a new board within a project.</summary>
public record CreateBoardRequest(string ProjectId, string Name);

/// <summary>Request to create a new work item (task, story, bug, or epic).</summary>
public record CreateWorkItemRequest(
    string ProjectId,
    string BoardId,
    string Title,
    string Description,
    string Type,
    string Priority,
    string? AssigneeId,
    string? SprintId,
    int? StoryPoints);

/// <summary>Request to update an existing work item.</summary>
public record UpdateWorkItemRequest(
    string Title,
    string Description,
    string Status,
    string Priority,
    string? AssigneeId,
    string? SprintId,
    int? StoryPoints);

/// <summary>Request to create a new sprint within a project.</summary>
public record CreateSprintRequest(string ProjectId, string Name, string Goal, DateTime? StartDate, DateTime? EndDate);
