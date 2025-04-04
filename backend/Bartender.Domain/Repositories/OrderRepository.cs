using AutoMapper;
using Bartender.Data;
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Orders;
using Bartender.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bartender.Domain.Repositories;

public class OrderRepository : Repository<Orders>, IOrderRepository
{
    private readonly IMapper _mapper;

    public OrderRepository(AppDbContext context, IMapper mapper) : base(context)
    {
        _mapper = mapper;
    }

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

    public async Task<List<Orders>> GetActiveOrdersByGuestIdAsync(Guid guestSessionId)
    {
        return await _dbSet
            .Include(o => o.Table)
            .Where(o => o.GuestSessionId == guestSessionId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Orders>> GetActiveOrdersByTableIdAsync(int tableId)
    {
        return await _dbSet
            .Include(o => o.Table)
            .Where(o => o.TableId == tableId && o.Status != OrderStatus.closed && o.Status != OrderStatus.cancelled)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Orders>> GetActiveByPlaceIdAsync(int placeId)
    {
        return await _dbSet
            .Include(o => o.Table)
            .Where(o => o.Table.PlaceId == placeId &&
                        o.Status != OrderStatus.closed && 
                        o.Status != OrderStatus.cancelled)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Orders>> GetPendingByPlaceIdAsync(int placeId)
    {
        return await _dbSet
            .Include(o => o.Table)
            .Where(o => o.Table.PlaceId == placeId &&
                        (o.Status == OrderStatus.created || 
                        o.Status == OrderStatus.payment_requested || 
                        o.Status == OrderStatus.approved))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Orders>> GetAllByPlaceIdAsync(int placeId)
    {
        return await _dbSet
            .Include(o => o.Table)
            .Where(o => o.Table.PlaceId == placeId && o.Status == OrderStatus.closed)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<BusinessOrdersDto>> GetAllOrdersByBusinessIdAsync(int businessId)
    {
        return await _dbSet
            .Where(o => o.Table.Place.BusinessId == businessId && o.Status == OrderStatus.closed)
            .Include(o => o.Table)
                .ThenInclude(t => t.Place)
            .GroupBy(o => o.Table.Place)
            .Select(g => new BusinessOrdersDto
            {
                Place = _mapper.Map<PlaceDto>(g.Key),
                Orders = _mapper.Map<List<OrderBaseDto>>(g.OrderByDescending(o => o.CreatedAt).ToList())
            })
            .ToListAsync();
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
