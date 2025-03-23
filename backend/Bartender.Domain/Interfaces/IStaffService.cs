
using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface IStaffService
{
    Task<Staff?> GetByIdAsync(int id, bool includeNavigations = false);
    Task<IEnumerable<Staff>> GetAllAsync();
    Task AddAsync(Staff staff);
    Task UpdateAsync(int id, Staff staff);
    Task DeleteAsync(int id);
}
