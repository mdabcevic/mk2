
using Bartender.Data.Models;
using Bartender.Domain.DTO;

namespace Bartender.Domain.Interfaces;

public interface IStaffService
{
    Task<StaffDto?> GetByIdAsync(int id, bool includeNavigations = false);
    Task<List<StaffDto>> GetAllAsync();
    Task AddAsync(UpsertStaffDto dto);
    Task UpdateAsync(int id, UpsertStaffDto dto);
    Task DeleteAsync(int id);
}
