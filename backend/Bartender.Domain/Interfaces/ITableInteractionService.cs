using Bartender.Data.Enums;
using Bartender.Domain.DTO.Table;

namespace Bartender.Domain.Interfaces;

public interface ITableInteractionService
{
    Task<TableScanDto> GetBySaltAsync(string salt, string? passphrase = null);
    Task ChangeStatusAsync(string token, TableStatus newStatus); // Guest/Waiter/Manager
}
