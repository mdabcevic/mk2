using System.Linq.Expressions;

namespace Bartender.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    /// <summary>
    /// Find an entity by its primary ID.
    /// </summary>
    /// <param name="id">Primary key in database.</param>
    /// <returns>Entity from table.</returns>
    Task<T?> GetByIdAsync(int id, bool includeNavigations = false);
    Task<T?> GetByKeyAsync(Expression<Func<T, bool>> key, params Expression<Func<T, object>>[]? includes);
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetAllWithDetailsAsync();
    Task AddAsync(T entity);
    Task AddMultipleAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);

    // Querying
    //IQueryable<T> Find(Expression<Func<T, bool>> predicate);
    IQueryable<T> QueryIncluding(params Expression<Func<T, object>>[] includes);

    IQueryable<T> Query();

    // Additional helpers
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);

    //void Detach(T entity);
}
