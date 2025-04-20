using Bartender.Data;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text;

namespace Bartender.Domain.Repositories;

public class OrderRepository : Repository<Orders>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }
    private const int pageSize = 30;
    private static Expression<Func<Orders, bool>> IsActiveOrderForPlace(int placeId)
    => o => o.Table.PlaceId == placeId &&
        o.Status != OrderStatus.closed &&
        o.Status != OrderStatus.cancelled;

    private static Expression<Func<Orders, bool>> IsPendingOrderForPlace(int placeId)
        => o => o.Table.PlaceId == placeId &&
                        (o.Status == OrderStatus.payment_requested ||
                        o.Status == OrderStatus.created ||
                        o.Status == OrderStatus.approved);

    public async Task CreateOrderWithItemsAsync(Orders order, List<ProductsPerOrder> items)
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
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new ApplicationException("Error creating order.", ex);
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

    public async Task<Orders?> getOrderById(int id)
    {
        return await _dbSet
            .Include(o => o.Table)
                .ThenInclude(t => t.Place)
            .Include(o => o.Products)
                .ThenInclude(p => p.MenuItem)
                    .ThenInclude(mi => mi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    private async Task<List<Orders>> GetOrdersAsync(
    Expression<Func<Orders, bool>> predicate,
    bool includeCustomer = false)
    {
        return await _dbSet
            .Include(o => o.Table)
            .Include(o => o.Products)
                .ThenInclude(p => p.MenuItem)
                    .ThenInclude(m => m.Product)
            .Include(o => o.Customer)
            .Where(predicate)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Orders>> GetActiveOrdersByGuestIdAsync(Guid guestSessionId)
    {
        return await GetOrdersAsync(o => o.GuestSessionId == guestSessionId);
    }

    public async Task<List<Orders>> GetActiveOrdersByTableIdAsync(int tableId)
    {
        return await GetOrdersAsync(o => o.TableId == tableId && o.Status != OrderStatus.closed && o.Status != OrderStatus.cancelled);
    }

    public async Task<List<Orders>> GetActiveByPlaceIdAsync(int placeId)
    {
        return await GetOrdersAsync(IsActiveOrderForPlace(placeId));
    }

    public async Task<List<Orders>> GetPendingByPlaceIdAsync(int placeId)
    {
        return await GetOrdersAsync(IsPendingOrderForPlace(placeId));
    }

    public async Task<List<Orders>> GetAllByPlaceIdAsync(int placeId)
    {
        return await GetOrdersAsync(o => o.Table.PlaceId == placeId && o.Status == OrderStatus.closed);
    }

    private async Task<(List<Orders>,int)> GetOrdersForGroupingAsync(
    Expression<Func<Orders, bool>> predicate, int page)
    {
        var query = _dbSet
        .Include(o => o.Table)
        .Include(o => o.Products)
            .ThenInclude(p => p.MenuItem)
                .ThenInclude(m => m.Product)
        .Where(predicate);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(Dictionary<OrderStatus, List<Orders>>,int)> GetActiveByPlaceIdGroupedAsync(int placeId,int page)
    {
        var (orders,total) = await GetOrdersForGroupingAsync(IsActiveOrderForPlace(placeId),page);
        return (orders.GroupBy(o => o.Status).ToDictionary(g => g.Key, g => g.ToList()), total);
    }

    public async Task<(Dictionary<OrderStatus, List<Orders>>, int)> GetPendingByPlaceIdGroupedAsync(int placeId, int page)
    {
        var (orders,total) = await GetOrdersForGroupingAsync(IsPendingOrderForPlace(placeId), page);
        return (orders.GroupBy(o => o.Status)
                    .ToDictionary(g => g.Key, g => g.ToList()) , total);
    }


    public async Task<Dictionary<Places, List<Orders>>> GetAllOrdersByBusinessIdAsync(int businessId)
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

    public async Task UpdateOrderWithItemsAsync(Orders existingOrder, List<ProductsPerOrder> newItems)
    {
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            _dbSet.Update(existingOrder);
            await context.SaveChangesAsync();

            var oldItems = await context.ProductsPerOrders
                .Where(i => i.OrderId == existingOrder.Id)
                .ToListAsync();

            var toAdd = new List<ProductsPerOrder>();
            var toDelete = new List<ProductsPerOrder>(oldItems);

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
