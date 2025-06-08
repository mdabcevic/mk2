using Bartender.Data;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bartender.Domain.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    protected readonly AppDbContext context;

    public AnalyticsRepository(AppDbContext context)
    {
        this.context = context;
    }

    public async Task<Dictionary<DayOfWeek, List<ProductPerOrder>>> GetProductsByDaysOfWeek(int businessId)
    {
        return await context.ProductsPerOrders
            .Include(o => o.Order)
                .ThenInclude(o => o.Table)
                    .ThenInclude(t => t.Place)
            .Include(o => o.MenuItem)
                .ThenInclude(mi => mi.Product)
            .Where(o => o.Order.Table.Place.BusinessId == businessId)
            .GroupBy(o => o.Order.CreatedAt.DayOfWeek)
            .ToDictionaryAsync(g => g.Key, g => g.ToList());
    }

    public async Task<List<ProductPerOrder>> GetOrderedProductsByBusinessId(int businessId, int? placeId = null, int? month = null, int? year = null)
    {
        var query = context.ProductsPerOrders
            .Include(o => o.Order)
                .ThenInclude(o => o.Table)
                    .ThenInclude(t => t.Place)
            .Include(o => o.MenuItem)
                .ThenInclude(mi => mi.Product)
            .Where(o => o.Order.Status == OrderStatus.closed);
       
        if (placeId != null)
            query = query.Where(o => o.Order.Table.PlaceId == placeId);
        
        else
            query = query.Where(o => o.Order.Table.Place.BusinessId == businessId);
        
        if (month != null)
            query = query.Where(o => o.Order.CreatedAt.Month == month);

        if (year != null)
            query = query.Where(o => o.Order.CreatedAt.Year == year);

        return await query.ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByBusinessId(int businessId, int? placeId = null, int ? month = null, int? year = null)
    {
        var query = context.Orders
            .Include(o => o.Table)
                .ThenInclude(t => t.Place)
                    .ThenInclude(p => p.City)
            .Where(o => o.Status == OrderStatus.closed);

        if (placeId != null)
            query = query.Where(o => o.Table.PlaceId == placeId);

        else
            query = query.Where(o => o.Table.Place.BusinessId == businessId);

        if (month != null)
            query = query.Where(o => o.CreatedAt.Month == month);

        if (year != null)
            query = query.Where(o => o.CreatedAt.Year == year);

        return await query.ToListAsync();
    }

    public async Task<List<Order>> GetOrdersByTime(DateTime dateTime, TimeFilter timeFilter, int businessId, int? placeId = null)
    {
        var query = context.Orders
            .Include(o => o.Table)
                .ThenInclude(t => t.Place)
            .Where(o => o.Status == OrderStatus.closed);

        if (placeId != null)
            query = query.Where(o => o.Table.PlaceId == placeId);

        else
            query = query.Where(o => o.Table.Place.BusinessId == businessId);

        Expression<Func<Order, bool>> filter = o => true;

        switch (timeFilter)
        {
            case TimeFilter.Day:
                filter = o => o.CreatedAt.Date == dateTime.Date;
                break;
            case TimeFilter.Month:
                filter = o => o.CreatedAt.Month == dateTime.Month && o.CreatedAt.Year == dateTime.Year;
                break;
            case TimeFilter.Year:
                filter = o => o.CreatedAt.Year == dateTime.Year;
                break;
        }

        query = query.Where(filter);

        return await query.ToListAsync();
    }

    public async Task<List<(Order order, WeatherData? weather)>> GetOrdersWithWeather(int businessId, int? placeId = null, int? month = null, int? year = null)
    {
        var query = context.Orders
            .Include(o => o.Table)
                .ThenInclude(t => t.Place)
                    .ThenInclude(p => p.City)
            .Include(o => o.Weather)
            .Where(o => o.Status == OrderStatus.closed);

        if (placeId != null)
            query = query.Where(o => o.Table.PlaceId == placeId);
        else
            query = query.Where(o => o.Table.Place.BusinessId == businessId);

        if (month != null)
            query = query.Where(o => o.CreatedAt.Month == month);

        if (year != null)
            query = query.Where(o => o.CreatedAt.Year == year);

        query = query.Where(o => o.Weather != null);
        var orders = await query.ToListAsync();

        return orders.Select(o => (o, o.Weather)).ToList();
    }

}
