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
    Task<T?> GetByKeyAsync(Expression<Func<T, bool>> key, bool includeNavigations = false, params Expression<Func<T, object>>[]? includes);
    Task<List<T>> GetAllAsync(bool? includeNavigations = false, params Expression<Func<T, object>>[]? orderBy);
    Task<List<T>> GetFilteredAsync(bool? includeNavigations = false, Expression<Func<T, bool>>? filterBy = null, bool orderByDescending = false, params Expression<Func<T, object>>[]? orderBy);
    Task AddAsync(T entity);
    Task AddMultipleAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task UpdateRangeAsync(IEnumerable<T> entities);
    Task DeleteAsync(T entity);
    Task DeleteRangeAsync(IEnumerable<T> entities);

    // Querying
    IQueryable<T> Query();
    IQueryable<T> QueryIncluding(params Expression<Func<T, object>>[] includes);

    // Additional helpers
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
}

