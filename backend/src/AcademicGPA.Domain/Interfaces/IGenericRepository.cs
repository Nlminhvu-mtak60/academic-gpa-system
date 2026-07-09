namespace AcademicGPA.Domain.Interfaces;

/// <summary>
/// Generic repository contract exposing common CRUD operations.
/// </summary>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Fetches a single entity record by its unique Guid primary key.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exposes IQueryable for complex queries.
    /// </summary>
    IQueryable<T> GetQueryable();

    /// <summary>
    /// Returns a list containing all active entity records.
    /// </summary>
    Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins tracking a new entity record.
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing tracked entity record.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Removes or soft-deletes an entity record.
    /// </summary>
    void Delete(T entity);
}
