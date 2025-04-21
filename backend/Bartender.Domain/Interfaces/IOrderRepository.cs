using Bartender.Data.Enums;
using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface IOrderRepository : IRepository<Orders>
{
    Task<Orders?> CreateOrderWithItemsAsync(Orders order, List<ProductsPerOrder> items);
    Task<Orders?> getOrderById(int id);
    Task<List<Orders>> GetActiveByPlaceIdAsync(int placeId);
    Task<List<Orders>> GetPendingByPlaceIdAsync(int placeId);
    Task<(List<Orders>, int)> GetAllByPlaceIdAsync(int placeId, int page);
    Task<List<Orders>> GetActiveOrdersByGuestIdAsync(Guid guestSessionId);
    Task<List<Orders>> GetActiveOrdersByTableIdAsync(int tableId);
    Task<(Dictionary<OrderStatus, List<Orders>>, int)> GetActiveByPlaceIdGroupedAsync(int placeId,int page);
    Task<(Dictionary<OrderStatus, List<Orders>>, int)> GetPendingByPlaceIdGroupedAsync(int placeId, int page);
    Task<Dictionary<Places, List<Orders>>> GetAllOrdersByBusinessIdAsync(int businessId);
    Task UpdateOrderWithItemsAsync(Orders existingOrder, List<ProductsPerOrder> newItems);
    Task SetTableOrdersAsClosedAsync(int tableId);
}
