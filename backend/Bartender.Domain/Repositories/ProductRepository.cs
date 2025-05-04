using Bartender.Data;
using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bartender.Domain.Repositories;

public class ProductRepository(AppDbContext context) : Repository<Product>(context), IProductRepository
{
    private const int pageSize = 30;

    public async Task<List<Product>> GetAllProductsAsync(
        int? businessId = null,
        bool? exclusive = null,
        bool? isAdmin = null,
        int? page = null)
    {
        Expression<Func<Product, bool>> predicate = FilterProductsPredicate(businessId, exclusive, isAdmin);
        return await _dbSet
            .Where(p => p.DeletedAt == null)
            .Where(predicate)
            .Include(p => p.Category)
            .OrderBy(p => p.Name ?? "")
            .Skip(((page ?? 1) - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Product>> GetProductsFilteredAsync(
        int? businessId = null,
        bool? exclusive = null,
        bool? isAdmin = null,
        string? name = null,
        string? category = null,
        int? page = null)
    {
        Expression<Func<Product, bool>> predicate = FilterProductsPredicate(businessId, exclusive, isAdmin, name, category);
        var query = _dbSet
            .Where(p => p.DeletedAt == null)
            .Where(predicate)
            .Include(p => p.Category);

        var orderedQuery = !string.IsNullOrWhiteSpace(name) ? 
            query
            .OrderByDescending(p => EF.Functions.ILike(p.Name, $"%{name}%"))
            .ThenBy(p => p.Name)
            : query.OrderBy(p => p.Name);

        return await orderedQuery
            .Skip(((page ?? 1) - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Dictionary<ProductCategory, List<Product>>> GetProductsGroupedAsync(
        int? businessId = null,
        bool? exclusive = null,
        bool? isAdmin = null,
        int? page = null)
    {
        Expression<Func<Product, bool>> predicate = FilterProductsPredicate(businessId, exclusive, isAdmin);
        var products = await _dbSet
            .Where(p => p.DeletedAt == null)
            .Where(predicate)
            .Include(p => p.Category)
            .OrderBy(p => p.Name ?? "")
            .ToListAsync();

        var grouped = products
            .Where(p => p.Category != null)
            .GroupBy(p => p.Category!)
            .Skip(((page ?? 1) - 1) * pageSize)
            .Take(pageSize)
            .ToDictionary(g => g.Key, g => g.ToList());

        return grouped;
    }

    public async Task<bool> ProductExists(string name, int? productId = null, int? businessId = null, string? volume = null)
    {
        return await _dbSet.AnyAsync(p =>
            (productId == null || p.Id != productId) &&
            (p.BusinessId == null || p.BusinessId == businessId) &&
            p.Name.ToLower() == name.ToLower() &&
            ((p.Volume == null && volume == null) ||
             p.Volume != null && volume != null && p.Volume.ToLower() == volume.ToLower()));
    }

    private static Expression<Func<Product, bool>> FilterProductsPredicate(
        int? businessId = null,
        bool? exclusive = null,
        bool? isAdmin = null,
        string? name = null,
        string? category = null
    )
    {
        return p =>
        (
            (isAdmin == false && exclusive == null && (p.BusinessId == businessId || p.BusinessId == null)) ||
            (exclusive == true && isAdmin == true && p.BusinessId != null) ||
            (exclusive == true && isAdmin == false && p.BusinessId == businessId) ||
            (exclusive == false && p.BusinessId == null)
        ) &&
        (
        string.IsNullOrEmpty(name)
        || EF.Functions.ILike(p.Name, $"%{name}%")
        || EF.Functions.ILike(p.Category.Name, $"%{name}%")
        )
        &&
        (
            string.IsNullOrEmpty(category)
            || EF.Functions.ILike(p.Category.Name, $"%{category}%")
        );
    }
}
