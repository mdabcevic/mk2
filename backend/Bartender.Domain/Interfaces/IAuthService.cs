
using Bartender.Domain.DTO.Staff;

namespace Bartender.Domain.Interfaces;

public interface IAuthService
{
    Task<string> LoginAsync(LoginStaffDto loginDto);
}
