using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.DTO;
using Bartender.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bartender.Domain.Services;

public class ValidationService(
    ICurrentUserContext currentUser,
    ITableSessionService tableSessionService,
    IRepository<Table> tableRepository,
    IRepository<Place> placeRepository,
    IRepository<Business> businessRepository,
    ILogger<ValidationService> logger) : IValidationService
{
    public async Task<ServiceResult> VerifyUserGuestAccess(int orderTableId)
    {
        if (currentUser.IsGuest && !await tableSessionService.HasActiveSessionAsync(orderTableId, currentUser.GetRawToken()))
        {
            logger.LogWarning("Guest access denied for TableId: {TableId}, Token: {Token}", orderTableId, currentUser.GetRawToken());
            return ServiceResult.Fail("You don't have access to manage orders for this table", ErrorType.Unauthorized);
        }
        else if (!currentUser.IsGuest)
        {
            var user = await currentUser.GetCurrentUserAsync();
            var table = await tableRepository.GetByIdAsync(orderTableId);

            if (!await VerifyUserPlaceAccess(table.PlaceId, user))
            {
                logger.LogWarning("Staff access denied. UserId: {UserId}, Username: {Username}, TablePlaceId: {PlaceId}", user.Id, user.Username, table.PlaceId);
                return ServiceResult.Fail("You don't have access to manage orders for this table", ErrorType.Unauthorized);
            }
        }
        return ServiceResult.Ok();
    }

    public async Task<bool> VerifyUserPlaceAccess(int targetPlaceId, Staff? user = null)
    {
        if (user == null)
        {
            user = await currentUser.GetCurrentUserAsync();
        }
        if (user!.Role == EmployeeRole.admin)
            return true;

        var targetPlace = await placeRepository.GetByIdAsync(targetPlaceId);

        if (targetPlace != null && targetPlace.BusinessId == user!.Place!.BusinessId && user!.Role == EmployeeRole.owner)
            return true;

        return targetPlaceId == user.PlaceId;
    }

    public async Task<bool> VerifyUserBusinessAccess(int businessId)
    {
        var user = await currentUser.GetCurrentUserAsync();

        if (user!.Role == EmployeeRole.admin)
            return true;

        if (user.Place == null)
        {
            logger.LogWarning("User {UserId} has no place assigned", user.Id);
            return false;
        }

        return businessId == user.Place.BusinessId;
    }
     // TODO - throw exceptions instead of returning ServiceResult
    public async Task<ServiceResult> EnsurePlaceExistsAsync(int placeId)
    {
        var exists = await placeRepository.ExistsAsync(p => p.Id == placeId);
        if (!exists)
            logger.LogWarning("Place not found. PlaceId: {PlaceId}", placeId);

        return exists
            ? ServiceResult.Ok()
            : ServiceResult.Fail($"Place with id {placeId} not found", ErrorType.NotFound);
    }

    public async Task<ServiceResult> EnsureBusinessExistsAsync(int businessId)
    {
        var exists = await businessRepository.ExistsAsync(b => b.Id == businessId);
        if (!exists)
            logger.LogWarning("Business not found. BusinessId: {BusinessId}", businessId);

        return exists
            ? ServiceResult.Ok()
            : ServiceResult.Fail($"Business with id {businessId} not found", ErrorType.NotFound);
    }

    public async Task<ServiceResult> EnsureTableExistsAsync(int tableId)
    {
        var exists = await tableRepository.ExistsAsync(b => b.Id == tableId);
        if (!exists)
            logger.LogWarning("Table not found. TableId: {TableId}", tableId);

        return exists
            ? ServiceResult.Ok()
            : ServiceResult.Fail($"Table with id {tableId} not found", ErrorType.NotFound);
    }
}
