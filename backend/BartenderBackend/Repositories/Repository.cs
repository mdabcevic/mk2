using Microsoft.EntityFrameworkCore;
using BartenderBackend.Models;

namespace BartenderBackend.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        this.context = context;
        _dbSet = this.context.Set<T>();
    }

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

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
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
