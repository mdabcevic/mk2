using Bartender.Domain.DTO;

namespace Bartender.Domain.Interfaces;

public interface IStaffService
{
    Task<ServiceResult<StaffDto>> GetByIdAsync(int id, bool includeNavigations = false);
    Task<ServiceResult<List<StaffDto>>> GetAllAsync();
    Task<ServiceResult> AddAsync(UpsertStaffDto dto);
    Task<ServiceResult> UpdateAsync(int id, UpsertStaffDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
