using Bartender.Data.Models;
using Bartender.Domain.Interfaces;

namespace Bartender.Domain.Services;

public class StaffService(IRepository<Staff> repository) : IStaffService
{
    public Task AddAsync(Staff staff)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Staff>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Staff?> GetByIdAsync(int id, bool includeNavigations = false)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Staff staff)
    {
        throw new NotImplementedException();
    }
}
