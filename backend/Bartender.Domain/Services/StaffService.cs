using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class StaffService(IRepository<Staff> repository, Microsoft.Extensions.Logging.ILogger<StaffService> logger) : IStaffService
{
    public async Task AddAsync(Staff staff)
    {
        if (await repository.ExistsAsync(s => s.Username == staff.Username))
        {
            logger.LogWarning("Staff created with ID: {StaffId}", staff.Id);
            throw new ArgumentException($"Staff with username '{staff.Username}' already exists.");
        }           
        await repository.AddAsync(staff);
        logger.LogInformation("Staff created with ID: {StaffId}", staff.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var staff = await repository.GetByIdAsync(id);
        if (staff == null)
        {
            logger.LogWarning("Attempted to delete non-existing staff with ID: {StaffId}", id);
            throw new KeyNotFoundException($"Staff with ID {id} not found.");
        }
        await repository.DeleteAsync(staff);
        logger.LogInformation("Staff deleted with ID: {StaffId}", id);
    }

    public async Task<IEnumerable<Staff>> GetAllAsync()
    {
        return await repository.GetAllAsync();
    }

    public async Task<Staff?> GetByIdAsync(int id, bool includeNavigations = false)
    {
        return await repository.GetByIdAsync(id, includeNavigations);
    }

    public async Task UpdateAsync(int id, Staff staff)
    {
        var staff2 = await repository.GetByIdAsync(id);
        if (staff2 == null)
        {
            logger.LogWarning("Attempted to update non-existing staff with ID: {StaffId}", id);
            throw new KeyNotFoundException($"Staff with ID {id} not found.");
        }
        await repository.UpdateAsync(staff);
        logger.LogInformation("Staff updated with ID: {StaffId}", id);
    }
}
