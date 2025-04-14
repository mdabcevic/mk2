using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Table;

namespace Bartender.Domain.Interfaces;

public interface ITableInteractionService
{
    Task<ServiceResult<TableScanDto>> GetBySaltAsync(string salt, string? passphrase = null);
    Task<ServiceResult> ChangeStatusAsync(string token, TableStatus newStatus); // Guest/Waiter/Manager
    Task<ServiceResult<TableScanDto>> TryJoinExistingSessionAsync(string salt, string submittedPassphrase);

}
