
namespace Bartender.Domain.Interfaces;

public interface ITableSessionService
{
    Task<bool> HasActiveSessionAsync(int tableId, string token);
}
