﻿using Bartender.Data.Enums;
using Bartender.Data.Models;
using Bartender.Domain.Interfaces;
using Bartender.Domain.Utility.Exceptions.NotFoundExceptions;
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
    public async Task<bool> VerifyUserGuestAccess(int orderTableId)
    {
        if (currentUser == null)
            return false;

        if (currentUser.IsGuest && await tableSessionService.HasActiveSessionAsync(orderTableId, currentUser.GetRawToken()))
            return true;

        else if (!currentUser.IsGuest)
        {
            var user = await currentUser.GetCurrentUserAsync();
            var table = await tableRepository.GetByIdAsync(orderTableId);

            if (table == null)
                return false;

            if (await VerifyUserPlaceAccess(table.PlaceId, user))
                return true;
        }
        return false;
    }

    public async Task<bool> VerifyProductAccess(int? businessId, bool isUpsertOperation, Staff? user = null)
    {
        user ??= await currentUser.GetCurrentUserAsync();

        if (user == null)
            return false;

        if (user.Role == EmployeeRole.admin)
            return true;

        if (!isUpsertOperation && businessId == null)
            return true;

        return user.Place?.BusinessId == businessId;
    }

    public async Task<bool> VerifyUserPlaceAccess(int targetPlaceId, Staff? user = null)
    {
        user ??= await currentUser.GetCurrentUserAsync();

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

    public async Task EnsurePlaceExistsAsync(int placeId)
    {
        var exists = await placeRepository.ExistsAsync(p => p.Id == placeId);
        if (!exists)
            throw new PlaceNotFoundException(placeId);
    }

    public async Task EnsureBusinessExistsAsync(int businessId)
    {
        var exists = await businessRepository.ExistsAsync(b => b.Id == businessId);

        if (!exists)
            throw new BusinessNotFoundException(businessId);
    }

    public async Task EnsureTableExistsAsync(int tableId)
    {
        var exists = await tableRepository.ExistsAsync(b => b.Id == tableId);

        if (!exists)
            throw new TableNotFoundException(tableId);
    }
}
