
using Bartender.Domain.DTO.Staff;

namespace Bartender.Domain.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<string>> LoginAsync(LoginStaffDto loginDto);
}
