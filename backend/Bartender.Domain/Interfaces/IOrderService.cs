
using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Orders;
using Bartender.Domain.DTO.Products;

namespace Bartender.Domain.Interfaces;

internal interface IOrderService
{
    Task<ServiceResult<OrderDto?>> GetByIdAsync(int id);
    Task<ServiceResult<List<OrderBaseDto>>> GetAllByBusinessIdAsync(int businessId);
    Task<ServiceResult<List<OrderDto>>> GetAllByPlaceIdAsync(int placeId, bool onlyActive = false, bool pending = false);
    Task<ServiceResult> AddAsync(UpsertOrderDto order);
    Task<ServiceResult> UpdateAsync(int id, UpsertOrderDto order);
    Task<ServiceResult> UpdateStatusAsync(int id, UpdateOrderStatusDto newStatus);
    Task<ServiceResult> DeleteAsync(int id);
}
