using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Orders;

namespace Bartender.Domain.Interfaces;

public interface IOrderService
{
    Task<ServiceResult<OrderDto?>> GetByIdAsync(int id);
    Task<ServiceResult<List<BusinessOrdersDto>>> GetAllByBusinessIdAsync(int businessId);
    Task<ServiceResult<List<OrderDto>>> GetAllClosedOrdersByPlaceIdAsync(int placeId,int page);
    Task<ServiceResult<List<OrderDto>>> GetAllActiveOrdersByPlaceIdAsync(int placeId, bool onlyWaitingForStaff = false);
    Task<ServiceResult<ListResponse<GroupedOrderStatusDto>>> GetAllActiveOrdersByPlaceIdGroupedAsync(int placeId, int page, bool onlyWaitingForStaff = false);
    Task<ServiceResult<List<OrderDto>>> GetActiveTableOrdersForUserAsync(bool userSpecific = true);
    Task<ServiceResult> AddAsync(UpsertOrderDto order);
    Task<ServiceResult> UpdateAsync(int id, UpsertOrderDto order);
    Task<ServiceResult> UpdateStatusAsync(int id, UpdateOrderStatusDto newStatus);
    Task<ServiceResult> DeleteAsync(int id);
}
