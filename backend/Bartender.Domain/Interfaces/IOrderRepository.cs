using Bartender.Data.Enums;
using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> CreateOrderWithItemsAsync(Order order, List<ProductPerOrder> items);
    Task<Order?> getOrderById(int id);
    Task<List<Order>> GetActiveByPlaceIdAsync(int placeId);
    Task<List<Order>> GetPendingByPlaceIdAsync(int placeId);
    Task<(List<Order>, int)> GetAllByPlaceIdAsync(int placeId, int page);
    Task<List<Order>> GetActiveOrdersByGuestIdAsync(Guid guestSessionId);
    Task<List<Order>> GetActiveOrdersByTableIdAsync(int tableId);
    Task<(Dictionary<OrderStatus, List<Order>>, int)> GetActiveByPlaceIdGroupedAsync(int placeId,int page);
    Task<(Dictionary<OrderStatus, List<Order>>, int)> GetPendingByPlaceIdGroupedAsync(int placeId, int page);
    Task<Dictionary<Place, List<Order>>> GetAllOrdersByBusinessIdAsync(int businessId);
    Task UpdateOrderWithItemsAsync(Order existingOrder, List<ProductPerOrder> newItems);
    Task SetTableOrdersAsClosedAsync(int tableId);
}
