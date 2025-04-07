using Microsoft.EntityFrameworkCore;
using Bartender.Domain.Interfaces;
using Bartender.Data;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage;

namespace Bartender.Domain.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        this.context = context;
        _dbSet = this.context.Set<T>();
    }

    // should include flag for choosing includes or not
    public async Task<T?> GetByIdAsync(int id, bool includeNavigations = false)
    {
        IQueryable<T> query = _dbSet;

        if (includeNavigations)
        {
            var navigationProperties = context.Model.FindEntityType(typeof(T))?.GetNavigations();

            if (navigationProperties != null)
            {
                foreach (var property in navigationProperties)
                {
                    if (!property.DeclaringEntityType.IsOwned())
                    {
                        query = query.Include(property.Name);
                    }
                }
            }
        }

        return await query.FirstOrDefaultAsync(entity => EF.Property<int>(entity, "Id") == id);
    }

    public async Task<T?> GetByIdAsync(
    int id,
    params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
            query = query.Include(include);

        return await query.FirstOrDefaultAsync(entity => EF.Property<int>(entity, "Id") == id);
    }

    public async Task<T?> GetByKeyAsync(Expression<Func<T, bool>> key, bool includeNavigations = false, params Expression<Func<T, object>>[]? includes)
    {
        var query = _dbSet.AsQueryable();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                if (include != null)
                    query = query.Include(include);
            }
        }

        if (includeNavigations)
            query = IncludeNavigations(query);

        return await query.FirstOrDefaultAsync(key);
    }


    public async Task<List<T>> GetAllAsync(bool? includeNavigations = false, params Expression<Func<T, object>>[]? orderBy)
    {
        var query = _dbSet.AsQueryable();

        if (includeNavigations != null && includeNavigations == true)
        {
            query = IncludeNavigations(query);
        }

        if (orderBy != null)
        {
            query = ApplyOrdering(query, orderBy);
        }

        return await query.ToListAsync();
    }

    public async Task<List<T>> GetFilteredAsync(
        bool? includeNavigations = false, 
        Expression<Func<T, bool>>? filterBy = null,
        bool orderByDescending = false,
        params Expression<Func<T, object>>[]? orderBy)
    {
        var query = _dbSet.AsQueryable();

        if (includeNavigations != null && includeNavigations == true)
            query = IncludeNavigations(query);

        if (filterBy != null)
            query = query.Where(filterBy);

        if (orderBy != null)
            query = ApplyOrdering(query, orderBy, orderByDescending);

        return await query.ToListAsync();
    }

    public IQueryable<T> IncludeNavigations(IQueryable<T> query)
    {
        var navigationProperties = context.Model.FindEntityType(typeof(T))?.GetNavigations();

        if (navigationProperties != null)
        {
            foreach (var property in navigationProperties)
            {
                if (!property.DeclaringEntityType.IsOwned())
                {
                    query = query.Include(property.Name);
                }
            }
        }
        return query;
    }

    public IQueryable<T> ApplyOrdering(IQueryable<T> query, Expression<Func<T, object>>[] orderBy, bool descending = false)
    {
        if (orderBy.Length == 0)
            return query;

        IOrderedQueryable<T> orderedQuery = descending 
            ? query.OrderByDescending(orderBy[0])
            : query.OrderBy(orderBy[0]);

        if (descending)
        {
            for (int i = 1; i < orderBy.Length; i++)
                orderedQuery = orderedQuery.ThenByDescending(orderBy[i]);
        }
        else
        {
            for (int i = 1; i < orderBy.Length; i++)
                orderedQuery = orderedQuery.ThenBy(orderBy[i]);
        }

        return orderedQuery;
    }

    // use only if you really need something very custom, otherwise rely on prebuild ones for consistency.
    public IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }

    public IQueryable<T> QueryIncluding(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;
        foreach (var include in includes)
        {
            if (include != null)
                query = query.Include(include);
        }
        return query;
    }

    //experimental from GPT
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    //experimental from GPT
    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public async Task AddMultipleAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        // track the entity through the context
        _dbSet.Attach(entity);
        // marks given state as modified
        context.Entry(entity).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
        await context.SaveChangesAsync();
    }

    public async Task SaveAsync()
    {
        await context.SaveChangesAsync();
    }
}
