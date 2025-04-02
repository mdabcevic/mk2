
using Bartender.Data.Models;
using Bartender.Domain.DTO.Orders;
using Bartender.Domain.DTO.Products;

namespace Bartender.Domain.Interfaces;

internal interface IOrderService
{
    Task<ServiceResult<OrderDto?>> GetByIdAsync(int id);
    Task<ServiceResult<List<OrderDto>>> GetAllAsync();
    Task<ServiceResult> AddAsync(UpsertOrderDto order);
    Task<ServiceResult> UpdateAsync(int id, UpsertOrderDto order);
    Task<ServiceResult> DeleteAsync(int id);
}
