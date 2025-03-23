using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class StaffService(
    IRepository<Staff> repository, 
    ILogger<StaffService> logger,
    IMapper mapper
    ) : IStaffService
{
    public async Task AddAsync(UpsertStaffDto dto)
    {
        if (await repository.ExistsAsync(s => s.Username == dto.Username))
        {
            logger.LogWarning("Cannot insert duplicate employee with username: {Username}", dto.Username);
            throw new ArgumentException($"Staff with username '{dto.Username}' already exists.");
        }   
        var employee = mapper.Map<Staff>(dto);
        await repository.AddAsync(employee);
        logger.LogInformation("Employee created with username: {Username}", dto.Username);
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

    public async Task<List<StaffDto>> GetAllAsync()
    {
        var staffList = await repository.GetAllAsync();
        return [.. staffList.Select(s => mapper.Map<StaffDto>(s))];
    }

    public async Task<StaffDto?> GetByIdAsync(int id, bool includeNavigations = false)
    {
        var staff = await repository.GetByIdAsync(id, includeNavigations);
        if (staff is null)
        {
            logger.LogWarning("Staff with ID {StaffId} was not found.", id);
            throw new KeyNotFoundException($"Staff with ID {id} not found.");
        }

        logger.LogInformation("Retrieved staff with ID {StaffId}.", id);
        return mapper.Map<StaffDto>(staff);
    }

    public async Task UpdateAsync(int id, UpsertStaffDto dto)
    {
        var employee = await repository.GetByIdAsync(id);
        if (employee == null)
        {
            logger.LogWarning("Attempted to update non-existing staff with ID: {StaffId}", id);
            throw new KeyNotFoundException($"Staff with ID {id} not found.");
        }

        mapper.Map(dto, employee);

        await repository.UpdateAsync(employee);
        logger.LogInformation("Staff updated with ID: {StaffId}", employee.Id);
    }
}
