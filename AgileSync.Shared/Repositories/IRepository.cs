using System.Linq.Expressions;
using AgileSync.Shared.Models;

namespace AgileSync.Shared.Repositories;

/// <summary>
/// Generic repository interface for CRUD operations on MongoDB entities.
/// </summary>
/// <typeparam name="T">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>Retrieves a single entity by its unique identifier.</summary>
    Task<T?> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>Retrieves all entities in the collection.</summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Retrieves entities matching the given predicate.</summary>
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    /// <summary>Inserts a new entity into the collection.</summary>
    Task CreateAsync(T entity, CancellationToken ct = default);

    /// <summary>Replaces an existing entity with updated data.</summary>
    Task UpdateAsync(T entity, CancellationToken ct = default);

    /// <summary>Deletes an entity by its unique identifier.</summary>
    Task DeleteAsync(string id, CancellationToken ct = default);
}
