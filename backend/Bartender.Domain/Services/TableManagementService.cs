
using AutoMapper;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Repositories;

namespace Bartender.Domain.Services;

public class TableManagementService : ITableManagementService
{
    /// <summary>
    /// Gets tables for current user’s place
    /// </summary>
    /// <returns></returns>
    public async Task<ServiceResult<List<TableDto>>> GetAllAsync()
    {
        var user = await currentUser.GetCurrentUserAsync();
        var tables = await repository.GetAllAsync();
        var filtered = tables
            .Where(t => t.PlaceId == user!.PlaceId)
            .Select(t => mapper.Map<TableDto>(t))
            .ToList();

        return ServiceResult<List<TableDto>>.Ok(filtered);
    }
}
