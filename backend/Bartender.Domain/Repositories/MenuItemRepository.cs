using Bartender.Data;
using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bartender.Domain.Repositories;

public class MenuItemRepository(AppDbContext context) : Repository<MenuItem>(context), IMenuItemRepository
{
    public async Task<List<MenuItem>> GetMenuItemsByPlaceIdAync(int placeId, bool onlyAvailable = false)
    {
        var query = _dbSet
            .Include(mi => mi.Product)
            .Include(mi => mi.Product.Category)
            .Where(mi => mi.PlaceId == placeId && mi.DeletedAt == null);

        if (onlyAvailable)
        {
            query = query.Where(mi => mi.IsAvailable);
        }

        return await query
            .OrderBy(mi => mi.Product != null ? mi.Product.Name : "")
            .ToListAsync();
    }

    public async Task<Dictionary<string, List<MenuItem>>> GetMenuItemsByPlaceIdGroupedAsync(int placeId, bool onlyAvailable = false)
    {
        var query = _dbSet
            .Include(mi => mi.Product)
            .Include(mi => mi.Product.Category)
            .Where(mi => mi.PlaceId == placeId && mi.DeletedAt == null);

        if (onlyAvailable)
        {
            query = query.Where(mi => mi.IsAvailable);
        }

        var menuItems = await query.ToListAsync();

        var groupedMenu = menuItems
            .GroupBy(mi => mi.Product?.Category?.Name ?? "Unknown")
            .ToDictionary(
            g => g.Key,
            g => g.OrderBy(mi => mi.Product != null ? mi.Product.Name : "").ToList());

        return groupedMenu;
    }

    public async Task<Dictionary<Place, List<MenuItem>>> GetAllGroupedByPlaceAsync()
    {
        var menuItems = await _dbSet
            .Include(mi => mi.Product)
            .Include(mi => mi.Product.Category)
            .Include(mi => mi.Place)
                .ThenInclude(p => p.Business)
            .Include(mi => mi.Place.City)
            .Where(mi => mi.DeletedAt == null)
            .ToListAsync();

        var groupedMenu = menuItems
            .GroupBy(mi => mi.Place!)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(mi => mi.Product != null ? mi.Product.Name : "").ToList());

        return groupedMenu;
    }
}
