using AgileSync.Shared.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace AgileSync.ProjectService.Models;

/// <summary>
/// Represents a Kanban-style board within a project, containing ordered columns.
/// </summary>
public class Board : BaseEntity
{
    /// <summary>ID of the project this board belongs to.</summary>
    [BsonElement("projectId")]
    public required string ProjectId { get; set; }

    /// <summary>Display name of the board.</summary>
    [BsonElement("name")]
    public required string Name { get; set; }

    /// <summary>Ordered list of columns on the board (defaults to To Do, In Progress, Done).</summary>
    [BsonElement("columns")]
    public List<BoardColumn> Columns { get; set; } =
    [
        new() { Name = "To Do", Order = 0 },
        new() { Name = "In Progress", Order = 1 },
        new() { Name = "Done", Order = 2 }
    ];
}

/// <summary>A single column on a Kanban board.</summary>
public class BoardColumn
{
    /// <summary>Display name of the column.</summary>
    [BsonElement("name")]
    public required string Name { get; set; }

    /// <summary>Sort order of the column (0-based, left to right).</summary>
    [BsonElement("order")]
    public int Order { get; set; }
}
