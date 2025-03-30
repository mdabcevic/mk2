using Bartender.Data.Enums;
using Bartender.Domain.DTO;

namespace Bartender.Domain.Interfaces;

public interface ITableService
{
    Task<ServiceResult<List<TableDto>>> GetAllAsync();
    Task<ServiceResult<TableDto>> GetByLabelAsync(string label);
    Task<ServiceResult> AddAsync(UpsertTableDto dto); // Managers only
    Task<ServiceResult> UpdateAsync(string label, UpsertTableDto dto); // Managers only
    Task<ServiceResult> DeleteAsync(string label); // Managers only

    Task<ServiceResult<TableScanDto>> GetBySaltAsync(string salt);
    Task<ServiceResult> ChangeStatusAsync(string token, TableStatus newStatus); // Guest/Waiter/Manager
    Task<ServiceResult> RegenerateSaltAsync(string label); // Managers only
    Task<ServiceResult> SwitchDisabledAsync(string label, bool flag);
}
