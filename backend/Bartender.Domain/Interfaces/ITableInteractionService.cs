using Bartender.Data.Enums;
using Bartender.Domain.DTO;

namespace Bartender.Domain.Interfaces;

public interface ITableInteractionService
{
    Task<ServiceResult<TableScanDto>> GetBySaltAsync(string salt);
    Task<ServiceResult> ChangeStatusAsync(string token, TableStatus newStatus); // Guest/Waiter/Manager
}
