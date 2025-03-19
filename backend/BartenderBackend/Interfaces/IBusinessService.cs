using BartenderBackend.Models;

namespace BartenderBackend.Interfaces;

public interface IBusinessService
{
    Task<Business?> GetByIdAsync(int id);
    Task<IEnumerable<Business>> GetAllAsync();
    Task AddAsync(Business business);
    Task UpdateAsync(Business business);
    Task DeleteAsync(int id);
}
