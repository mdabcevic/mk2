using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface IJwtService
{
    string GenerateGuestToken(int tableId, Guid sessionId, DateTime expiresAt, string passphrase);
    string GenerateStaffToken(Staff staff);
}
