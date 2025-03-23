namespace Bartender.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    /// <summary>
    /// Find an entity by its primary ID.
    /// </summary>
    /// <param name="id">Primary key in database.</param>
    /// <returns>Entity from table.</returns>
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetAllIncludingNavigationAsync();
    Task AddAsync(T entity);
    Task AddMultipleAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    IQueryable<T> Query();
}
