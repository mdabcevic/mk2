
namespace Bartender.Domain.Interfaces;

public interface ITableSessionService
{
    Task<bool> HasActiveSessionAsync(int tableId);
    Task<bool> IsSameTokenAsActiveAsync(int tableId, string token);
    Task<bool> CanResumeExpiredSessionAsync(int tableId, string token);
}
