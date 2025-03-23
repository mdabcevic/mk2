namespace Bartender.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    /// <summary>
    /// Find an entity by its primary ID.
    /// </summary>
    /// <param name="id">Primary key in database.</param>
    /// <returns>Entity from table.</returns>
    Task<T?> GetByIdAsync(int id, bool includeNavigations = false);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
