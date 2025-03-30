using Bartender.Data.Enums;
using Bartender.Domain.DTO;

namespace Bartender.Domain.Interfaces;

public interface ITableService
{
    Task<ServiceResult<List<TableDto>>> GetAllAsync();
    Task<ServiceResult<TableDto>> GetByIdAsync(int id);
    Task<ServiceResult> AddAsync(UpsertTableDto dto); // Managers only
    Task<ServiceResult> UpdateAsync(int id, UpsertTableDto dto); // Managers only
    Task<ServiceResult> DeleteAsync(int id); // Managers only

    Task<ServiceResult<TableDto>> GetBySaltAsync(string salt);
    Task<ServiceResult> ChangeStatusAsync(int id, TableStatus newStatus); // Guest/Waiter/Manager
    Task<ServiceResult> RegenerateSaltAsync(int id); // Managers only
    Task<ServiceResult> EnableAsync(int id); // Managers only
    Task<ServiceResult> DisableAsync(int id); // Managers only

    // Optional future additions
    // Task<ServiceResult> StartSessionAsync(int tableId); 
    // Task<ServiceResult> EndSessionAsync(int tableId);
}
