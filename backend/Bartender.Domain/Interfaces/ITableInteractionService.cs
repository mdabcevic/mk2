using Bartender.Data.Enums;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Table;

namespace Bartender.Domain.Interfaces;

public interface ITableInteractionService
{
    Task<ServiceResult<TableScanDto>> GetBySaltAsync(string salt, string? passphrase = null);
    Task<ServiceResult> ChangeStatusAsync(string token, TableStatus newStatus); // Guest/Waiter/Manager
}
