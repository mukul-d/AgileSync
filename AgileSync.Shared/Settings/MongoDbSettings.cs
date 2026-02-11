namespace AgileSync.Shared.Settings;

/// <summary>
/// Configuration settings for MongoDB connection, bound from the "MongoDb" config section.
/// </summary>
public sealed class MongoDbSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "MongoDb";

    /// <summary>The MongoDB connection string.</summary>
    public required string ConnectionString { get; init; }

    /// <summary>The name of the database to connect to.</summary>
    public required string DatabaseName { get; init; }
}
