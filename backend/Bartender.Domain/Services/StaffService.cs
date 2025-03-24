using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class StaffService(
    IRepository<Staff> repository, 
    IRepository<Places> placesRepository,
    ILogger<StaffService> logger,
    IMapper mapper
    ) : IStaffService
{
    public async Task AddAsync(UpsertStaffDto dto /*, int currentUserId */)
    {
        //await EnsureSameBusinessAsync(currentUserId, dto.PlaceId);

        if (!await placesRepository.ExistsAsync(p => p.Id == dto.PlaceId))
        {
            logger.LogWarning("Place with ID {PlaceId} does not exist.", dto.PlaceId);
            throw new ArgumentException($"Place with ID {dto.PlaceId} does not exist.");
        }

        if (await repository.ExistsAsync(s => s.Username == dto.Username))
        {
            logger.LogWarning("Cannot insert duplicate employee with username: {Username}", dto.Username);
            throw new ArgumentException($"Staff with username '{dto.Username}' already exists.");
        }

        var employee = mapper.Map<Staff>(dto);
        await repository.AddAsync(employee);
        logger.LogInformation("Employee created with username: {Username}", dto.Username);
    }

    public async Task DeleteAsync(int id /*, int currentUserId */)
    {
        var staff = await repository.GetByIdAsync(id);
        if (staff == null)
        {
            logger.LogWarning("Attempted to delete non-existing staff with ID: {StaffId}", id);
            throw new KeyNotFoundException($"Staff with ID {id} not found.");
        }

        //await EnsureSameBusinessAsync(currentUserId, staff.PlaceId);

        await repository.DeleteAsync(staff);
        logger.LogInformation("Staff deleted with ID: {StaffId}", id);
    }

    public async Task<List<StaffDto>> GetAllAsync(/*int currentUserId */)
    {
        //var currentUser = await repository.GetByIdAsync(currentUserId);
        var staffList = await repository.GetAllAsync();

        //return [.. staffList
        //    .Where(s => s.PlaceId == currentUser.PlaceId) // optional: filter to same place only
        //    .Select(s => mapper.Map<StaffDto>(s))];
        return [.. staffList.Select(s => mapper.Map<StaffDto>(s))];
    }

    public async Task<StaffDto?> GetByIdAsync(int id, /*int currentUserId,*/ bool includeNavigations = false)
    {
        var staff = await repository.GetByIdAsync(id, includeNavigations);
        if (staff is null)
        {
            logger.LogWarning("Staff with ID {StaffId} was not found.", id);
            throw new KeyNotFoundException($"Staff with ID {id} not found.");
        }

        //await EnsureSameBusinessAsync(currentUserId, staff.PlaceId);

        logger.LogInformation("Retrieved staff with ID {StaffId}.", id);
        return mapper.Map<StaffDto>(staff);
    }

    public async Task UpdateAsync(int id, UpsertStaffDto dto/*, int currentUserId*/)
    {
        var employee = await repository.GetByIdAsync(id);
        if (employee == null)
        {
            logger.LogWarning("Attempted to update non-existing staff with ID: {StaffId}", id);
            throw new KeyNotFoundException($"Staff with ID {id} not found.");
        }

        //await EnsureSameBusinessAsync(currentUserId, dto.PlaceId);

        mapper.Map(dto, employee);
        await repository.UpdateAsync(employee);
        logger.LogInformation("Staff updated with ID: {StaffId}", employee.Id);
    }

    private async Task EnsureSameBusinessAsync(int currentUserId, int targetPlaceId)
    {
        var user = await repository.GetByIdAsync(currentUserId, includeNavigations: true)
            ?? throw new UnauthorizedAccessException("User not found.");

        var userPlace = await placesRepository.GetByIdAsync(user.PlaceId);
        var targetPlace = await placesRepository.GetByIdAsync(targetPlaceId);

        if (userPlace?.BusinessId != targetPlace?.BusinessId)
            throw new UnauthorizedAccessException("Cross-business access denied.");
    }
}
