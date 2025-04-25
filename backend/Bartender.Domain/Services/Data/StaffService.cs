using AutoMapper;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Staff;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services.Data;

public class StaffService(
    IRepository<Staff> repository, 
    ILogger<StaffService> logger,
    ICurrentUserContext currentUser,
    IMapper mapper
    ) : IStaffService
{
    public async Task<ServiceResult> AddAsync(UpsertStaffDto dto)
    {
        if (!await IsSameBusinessAsync(dto.PlaceId))
            return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        if (await repository.ExistsAsync(s => s.Username == dto.Username))
        {
            logger.LogWarning("Username conflict: {Username}", dto.Username);
            return ServiceResult.Fail($"Staff with username '{dto.Username}' already exists.", ErrorType.Conflict);
        }

        var employee = mapper.Map<Staff>(dto);
        await repository.AddAsync(employee);
        logger.LogInformation("User {UserId} created new staff: {Username}", currentUser.UserId, dto.Username);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var staff = await repository.GetByIdAsync(id);
        if (staff == null)
            return ServiceResult.Fail($"Staff with ID {id} not found.", ErrorType.NotFound);

        if (!await IsSameBusinessAsync(staff.PlaceId))
            return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        await repository.DeleteAsync(staff);
        logger.LogInformation("Staff deleted with ID: {StaffId}", id);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<List<StaffDto>>> GetAllAsync()
    {
        var user = await currentUser.GetCurrentUserAsync();
        var staffList = await repository.GetAllAsync(); //TODO: filter directly on Database with IQueryable?

        var filtered = staffList
            .Where(s => s.PlaceId == user.PlaceId)
            .Select(s => mapper.Map<StaffDto>(s))
            .ToList();

        return ServiceResult<List<StaffDto>>.Ok(filtered);
    }

    public async Task<ServiceResult<StaffDto>> GetByIdAsync(int id, bool includeNavigations = false)
    {
        var staff = await repository.GetByIdAsync(id, includeNavigations);
        if (staff is null)
            return ServiceResult<StaffDto>.Fail($"Staff with ID {id} not found.", ErrorType.NotFound);

        if (!await IsSameBusinessAsync(staff.PlaceId))
            return ServiceResult<StaffDto>.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        var dto = mapper.Map<StaffDto>(staff);
        return ServiceResult<StaffDto>.Ok(dto);
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpsertStaffDto dto)
    {
        var employee = await repository.GetByIdAsync(id);
        if (employee == null)
            return ServiceResult.Fail($"Staff with ID {id} not found.", ErrorType.NotFound);

        if (!await IsSameBusinessAsync(dto.PlaceId))
            return ServiceResult.Fail("Cross-business access denied.", ErrorType.Unauthorized);

        mapper.Map(dto, employee);
        await repository.UpdateAsync(employee);
        logger.LogInformation("Staff updated with ID: {StaffId}", employee.Id);
        return ServiceResult.Ok();
    }

    private async Task<bool> IsSameBusinessAsync(int targetPlaceId)
    {
        var user = await currentUser.GetCurrentUserAsync();
        return targetPlaceId == user.PlaceId;
    }
}
