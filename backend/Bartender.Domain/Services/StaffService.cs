using Bartender.Data.Models;
using Bartender.Domain.Interfaces;

namespace Bartender.Domain.Services;

public class StaffService(IRepository<Staff> repository) : IStaffService
{
    public async Task AddAsync(Staff staff)
    {
        await repository.AddAsync(staff);
    }

    public async Task DeleteAsync(int id)
    {
        var staff = await repository.GetByIdAsync(id);
        if (staff != null)
            await repository.DeleteAsync(staff);
    }

    public async Task<IEnumerable<Staff>> GetAllAsync()
    {
        return await repository.GetAllAsync();
    }

    public async Task<Staff?> GetByIdAsync(int id, bool includeNavigations = false)
    {
        return await repository.GetByIdAsync(id, includeNavigations);
    }

    public async Task UpdateAsync(Staff staff)
    {
        await repository.UpdateAsync(staff);
    }
}
