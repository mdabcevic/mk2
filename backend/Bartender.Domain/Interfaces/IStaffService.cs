
using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface IStaffService
{
    Task<Staff?> GetByIdAsync(int id, bool includeNavigations = false);
    Task<IEnumerable<Staff>> GetAllAsync();
    Task AddAsync(UpsertStaffDto dto);
    Task UpdateAsync(int id, UpsertStaffDto dto);
    Task DeleteAsync(int id);
}
