using Bartender.Data;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Bartender.Domain.Repositories;

public class OrderRepository(AppDbContext context) : Repository<Order>(context), IOrderRepository
{
    private const int pageSize = 15;
    private static Expression<Func<Order, bool>> IsActiveOrderForPlace(int placeId)
    => o => o.Table.PlaceId == placeId &&
        o.Status != OrderStatus.closed &&
        o.Status != OrderStatus.cancelled;

    private static Expression<Func<Order, bool>> IsPendingOrderForPlace(int placeId)
        => o => o.Table.PlaceId == placeId &&
                        (o.Status == OrderStatus.payment_requested ||
                        o.Status == OrderStatus.created ||
                        o.Status == OrderStatus.approved);

    public async Task<Order?> CreateOrderWithItemsAsync(Order order, List<ProductPerOrder> items)
    {
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            await _dbSet.AddAsync(order);
            await context.SaveChangesAsync();

            items.ForEach(i => i.OrderId = order.Id);
            await context.ProductsPerOrders.AddRangeAsync(items);
            await context.SaveChangesAsync();
            
            await transaction.CommitAsync();
            await context.Entry(order).Reference(o => o.Table).LoadAsync();
            return order;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new ApplicationException("Error creating order.", ex); //TODO: custom exception?
        }
    }

    public async Task SetTableOrdersAsClosedAsync(int tableId)
    {
        var orders = await _dbSet
            .Where(o => o.TableId == tableId && 
            o.Status != OrderStatus.closed && 
            o.Status != OrderStatus.cancelled).ToListAsync();

        if (!orders.Any())
            return;

        foreach (var order in orders)
            order.Status = OrderStatus.closed;

        await context.SaveChangesAsync();
    }

    public async Task<Order?> getOrderById(int id)
    {
        return await _dbSet
            .Include(o => o.Table)
                .ThenInclude(t => t.Place)
            .Include(o => o.Products)
                .ThenInclude(p => p.MenuItem)
                    .ThenInclude(mi => mi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    private async Task<List<Order>> GetOrdersAsync(
    Expression<Func<Order, bool>> predicate,
    bool includeCustomer = false, int? page = null, int? size = null)
    {
        int _pageSize = (int)(size != null ? (size > pageSize ? pageSize : size) : pageSize);
        return await _dbSet
            .Include(o => o.Table)
            .Include(o => o.Products)
                .ThenInclude(p => p.MenuItem)
                    .ThenInclude(m => m.Product)
            .Include(o => o.Customer)
            .Where(predicate)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) ?? 0 * _pageSize)
            .Take(_pageSize)
            .ToListAsync();
    }

    private async Task<int> TotalCountAsync(
    Expression<Func<Order, bool>> predicate,
    bool includeCustomer = false)
    {
        return await _dbSet
            .Include(o => o.Table)
            .Include(o => o.Products)
                .ThenInclude(p => p.MenuItem)
                    .ThenInclude(m => m.Product)
            .Include(o => o.Customer)
            .Where(predicate)
            .CountAsync();
    }

    public async Task<List<Order>> GetActiveOrdersByGuestIdAsync(Guid guestSessionId)
    {
        return await GetOrdersAsync(o => o.GuestSessionId == guestSessionId);
    }

    public async Task<List<Order>> GetActiveOrdersByTableIdAsync(int tableId)
    {
        return await GetOrdersAsync(o => o.TableId == tableId && o.Status != OrderStatus.closed && o.Status != OrderStatus.cancelled);
    }

    public async Task<List<Order>?> GetCurrentOrdersByTableLabelAsync(string tableLabel)
    {
        return await _dbSet
        .Include(o => o.Table)
            .ThenInclude(t => t.Place)
        .Include(o => o.Products)
            .ThenInclude(p => p.MenuItem)
                .ThenInclude(m => m.Product)
        .Include(o => o.GuestSession)
        .Where(o => o.Table!.Label == tableLabel && o.GuestSession != null && o.GuestSession.IsValid)
        .OrderByDescending(o => o.CreatedAt)
        .ToListAsync();
    }

    public async Task<List<Order>> GetActiveByPlaceIdAsync(int placeId)
    {
        return await GetOrdersAsync(IsActiveOrderForPlace(placeId));
    }

    public async Task<List<Order>> GetPendingByPlaceIdAsync(int placeId)
    {
        return await GetOrdersAsync(IsPendingOrderForPlace(placeId));
    }

    public async Task<(List<Order>,int)> GetAllByPlaceIdAsync(int placeId, int page, int size)
    {
        return (await GetOrdersAsync(o => o.Table.PlaceId == placeId && o.Status == OrderStatus.closed,false,page,size),
                await TotalCountAsync(o => o.Table.PlaceId == placeId && o.Status == OrderStatus.closed));
    }

    private async Task<(List<Order>,int)> GetOrdersForGroupingAsync(
    Expression<Func<Order, bool>> predicate, int page, int? size)
    {   var _pageSize = (int)(size != null ? (size > pageSize ? pageSize : size) : pageSize);
        var query = _dbSet
        .Include(o => o.Table)
        .Include(o => o.Products)
            .ThenInclude(p => p.MenuItem)
                .ThenInclude(m => m.Product)
        .Where(predicate);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page > 0 ? (page - 1) : 0) * _pageSize)
            .Take(_pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(Dictionary<OrderStatus, List<Order>>,int)> GetActiveByPlaceIdGroupedAsync(int placeId,int page, int size)
    {
        var (orders,total) = await GetOrdersForGroupingAsync(IsActiveOrderForPlace(placeId),page,size);
        return (orders.GroupBy(o => o.Status).ToDictionary(g => g.Key, g => g.ToList()), total);
    }

    public async Task<(Dictionary<OrderStatus, List<Order>>, int)> GetPendingByPlaceIdGroupedAsync(int placeId, int page, int size)
    {
        var (orders,total) = await GetOrdersForGroupingAsync(IsPendingOrderForPlace(placeId), page, size);
        return (orders.GroupBy(o => o.Status)
                    .ToDictionary(g => g.Key, g => g.ToList()) , total);
    }


    public async Task<Dictionary<Place, List<Order>>> GetAllOrdersByBusinessIdAsync(int businessId)
    {
        var orders = await _dbSet
            .Include(o => o.Table.Place.City)
            .Include(o => o.Table.Place.Business)
            .Include(o => o.Products)
                .ThenInclude(p => p.MenuItem)
                    .ThenInclude(m => m.Product)
            .Where(o => o.Table.Place.BusinessId == businessId && o.Status == OrderStatus.closed)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders
            .GroupBy(o => o.Table.Place)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task UpdateOrderWithItemsAsync(Order existingOrder, List<ProductPerOrder> newItems)
    {
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            _dbSet.Update(existingOrder);
            await context.SaveChangesAsync();

            var oldItems = await context.ProductsPerOrders
                .Where(i => i.OrderId == existingOrder.Id)
                .ToListAsync();

            var toAdd = new List<ProductPerOrder>();
            var toDelete = new List<ProductPerOrder>(oldItems);

            foreach (var newItem in newItems)
            {
                newItem.OrderId = existingOrder.Id;

                var matchingOld = oldItems.FirstOrDefault(oi => oi.MenuItemId == newItem.MenuItemId);

                if (matchingOld != null)
                {
                    if (newItem.Count != matchingOld.Count || newItem.Price != matchingOld.Price || newItem.Discount != matchingOld.Discount)
                    {
                        matchingOld.Count = newItem.Count;
                        matchingOld.Price = newItem.Price;
                        matchingOld.Discount = newItem.Discount;
                    }
                    toDelete.Remove(matchingOld);
                }
                else
                {
                    toAdd.Add(newItem);
                }
            }

            if (toDelete.Count > 0)
                context.ProductsPerOrders.RemoveRange(toDelete);

            if (toAdd.Count > 0)
                await context.ProductsPerOrders.AddRangeAsync(toAdd); 

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new ApplicationException("Error updating order and items", ex);
        }
    }
}
