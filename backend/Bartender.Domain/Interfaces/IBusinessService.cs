using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface IBusinessService
{
    Task<Businesses?> GetByIdAsync(int id);
    Task<IEnumerable<Businesses>> GetAllAsync();
    Task AddAsync(Businesses business);
    Task UpdateAsync(Businesses business);
    Task DeleteAsync(int id);
}
