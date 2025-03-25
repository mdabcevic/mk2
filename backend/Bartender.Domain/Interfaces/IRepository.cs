using System.Linq.Expressions;

namespace Bartender.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    /// <summary>
    /// Find an entity by its primary ID.
    /// </summary>
    /// <param name="id">Primary key in database.</param>
    /// <returns>Entity from table.</returns>
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetByKeyAsync(Expression<Func<T, bool>> key, params Expression<Func<T, object>>[]? includes);
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetAllWithDetailsAsync();
    Task AddAsync(T entity);
    Task AddMultipleAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    IQueryable<T> Query();
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    IQueryable<T> QueryIncluding(params Expression<Func<T, object>>[] includes);
}
