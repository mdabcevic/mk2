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
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);

    // Querying
    //IQueryable<T> Find(Expression<Func<T, bool>> predicate);
    IQueryable<T> QueryIncluding(params Expression<Func<T, object>>[] includes);

    // Additional helpers
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);

    //void Detach(T entity);
}
