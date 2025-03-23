using Microsoft.EntityFrameworkCore;
using Bartender.Domain.Interfaces;
using Bartender.Data;
using System.Linq.Expressions;

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
    public async Task<T?> GetByIdAsync(int id)
    {
        var query = _dbSet.AsQueryable();

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
        return await query.FirstOrDefaultAsync(entity => EF.Property<int>(entity, "Id") == id);
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<List<T>> GetAllWithDetailsAsync()
    {
        var query = _dbSet.AsQueryable();
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
        return await query.ToListAsync();
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

    public async Task SaveAsync()
    {
        await context.SaveChangesAsync();
    }
}
