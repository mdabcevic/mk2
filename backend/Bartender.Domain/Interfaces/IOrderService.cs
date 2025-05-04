using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Order;

namespace Bartender.Domain.Interfaces;

public interface IOrderService
{
    Task<OrderDto?> GetByIdAsync(int id,bool skipValidation);
    Task<List<OrderDto>> GetCurrentOrdersByTableLabelAsync(string tableLabel);
    Task<List<BusinessOrdersDto>> GetAllByBusinessIdAsync(int businessId);
    Task<ListResponse<OrderDto>> GetAllClosedOrdersByPlaceIdAsync(int placeId,int page);
    Task<List<OrderDto>> GetAllActiveOrdersByPlaceIdAsync(int placeId,bool onlyWaitingForStaff = false);
    Task<ListResponse<GroupedOrderStatusDto>> GetAllActiveOrdersByPlaceIdGroupedAsync(int placeId, int page, bool onlyWaitingForStaff = false);
    Task<List<OrderDto>> GetActiveTableOrdersForUserAsync(bool userSpecific = true);
    Task AddAsync(UpsertOrderDto order);
    Task UpdateAsync(int id, UpsertOrderDto order);
    Task UpdateStatusAsync(int id, UpdateOrderStatusDto newStatus);
    Task DeleteAsync(int id);
}
