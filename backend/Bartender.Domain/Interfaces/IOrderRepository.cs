using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Orders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartender.Domain.Interfaces;

public interface IOrderRepository : IRepository<Orders>
{
    Task CreateOrderWithItemsAsync(Orders order, List<ProductsPerOrder> items);
    Task<Orders?> getOrderById(int id);
    Task<List<Orders>> GetActiveByPlaceIdAsync(int placeId);
    Task<List<Orders>> GetPendingByPlaceIdAsync(int placeId);
    Task<List<Orders>> GetAllByPlaceIdAsync(int placeId);
    Task<List<Orders>> GetActiveOrdersByGuestIdAsync(Guid guestSessionId);
    Task<List<Orders>> GetActiveOrdersByTableIdAsync(int tableId);
    Task<List<GroupedOrderStatusDto>> GetActiveByPlaceIdGroupedAsync(int placeId);
    Task<List<GroupedOrderStatusDto>> GetPendingByPlaceIdGroupedAsync(int placeId);
    Task<List<BusinessOrdersDto>> GetAllOrdersByBusinessIdAsync(int businessId);
    Task UpdateOrderWithItemsAsync(Orders existingOrder, List<ProductsPerOrder> newItems);
    Task SetTableOrdersAsClosedAsync(int tableId);
}
