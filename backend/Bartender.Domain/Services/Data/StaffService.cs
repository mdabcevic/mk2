using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO.Staff;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Bartender.Domain.utility.Exceptions;

namespace Bartender.Domain.Services.Data;

public class StaffService(
    IRepository<Staff> repository, 
    ILogger<StaffService> logger,
    ICurrentUserContext currentUser,
    IMapper mapper
    ) : IStaffService
{
    public async Task AddAsync(UpsertStaffDto dto)
    {
        if (!await IsSameBusinessAsync(dto.PlaceId))
            throw new UnauthorizedBusinessAccessException();

        if (await repository.ExistsAsync(s => s.Username == dto.Username))
        {
            throw new ConflictException($"Staff with username '{dto.Username}' already exists.");
        }

        var employee = mapper.Map<Staff>(dto);
        await repository.AddAsync(employee);
        logger.LogInformation("User {UserId} created new staff: {Username}", currentUser.UserId, dto.Username);
    }

    public async Task DeleteAsync(int id)
    {
        var staff = await repository.GetByIdAsync(id);
        if (staff == null)
            throw new StaffNotFoundException(id);

        if (!await IsSameBusinessAsync(staff.PlaceId))
            throw new UnauthorizedPlaceAccessException(id);

        await repository.DeleteAsync(staff);
        logger.LogInformation("Staff deleted with ID: {StaffId}", id);
    }

    public async Task<List<StaffDto>> GetAllAsync()
    {
        var user = await currentUser.GetCurrentUserAsync();
        var staffList = await repository.GetAllAsync(); //TODO: filter directly on Database with IQueryable?

        var filtered = staffList
            .Where(s => s.PlaceId == user.PlaceId)
            .Select(s => mapper.Map<StaffDto>(s))
            .ToList();

        return filtered;
    }

    public async Task<StaffDto> GetByIdAsync(int id, bool includeNavigations = false)
    {
        var staff = await repository.GetByIdAsync(id, includeNavigations);
        if (staff is null)
            throw new StaffNotFoundException(id);

        if (!await IsSameBusinessAsync(staff.PlaceId))
            throw new UnauthorizedPlaceAccessException(id);

        var dto = mapper.Map<StaffDto>(staff);
        return dto;
    }

    public async Task UpdateAsync(int id, UpsertStaffDto dto)
    {
        var employee = await repository.GetByIdAsync(id);
        if (employee == null)
            throw new StaffNotFoundException(id);

        if (!await IsSameBusinessAsync(dto.PlaceId))
            throw new UnauthorizedPlaceAccessException(id);

        mapper.Map(dto, employee);
        await repository.UpdateAsync(employee);
        logger.LogInformation("Staff updated with ID: {StaffId}", employee.Id);
    }

    private async Task<bool> IsSameBusinessAsync(int targetPlaceId)
    {
        var user = await currentUser.GetCurrentUserAsync();
        return targetPlaceId == user.PlaceId;
    }
}
