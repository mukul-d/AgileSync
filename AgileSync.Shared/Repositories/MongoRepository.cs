using System.Linq.Expressions;
using AgileSync.Shared.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace AgileSync.Shared.Repositories;

/// <summary>
/// MongoDB implementation of <see cref="IRepository{T}"/>.
/// Uses a primary constructor to inject the database and collection name.
/// </summary>
/// <typeparam name="T">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
public class MongoRepository<T>(IMongoDatabase database, string collectionName) : IRepository<T>
    where T : BaseEntity
{
    /// <summary>The underlying MongoDB collection.</summary>
    protected readonly IMongoCollection<T> Collection = database.GetCollection<T>(collectionName);

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(string id, CancellationToken ct = default) =>
        await Collection.AsQueryable()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await Collection.AsQueryable().ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await Collection.AsQueryable()
            .Where(predicate)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task CreateAsync(T entity, CancellationToken ct = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await Collection.InsertOneAsync(entity, cancellationToken: ct);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await Collection.ReplaceOneAsync(
            Builders<T>.Filter.Eq(e => e.Id, entity.Id),
            entity,
            cancellationToken: ct);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id, CancellationToken ct = default) =>
        await Collection.DeleteOneAsync(
            Builders<T>.Filter.Eq(e => e.Id, id),
            cancellationToken: ct);
}
