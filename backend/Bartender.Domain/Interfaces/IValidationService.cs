using Bartender.Data.Models;
using Bartender.Domain.DTO;

namespace Bartender.Domain.Interfaces;

public interface IValidationService
{
    Task VerifyUserGuestAccess(int orderTableId);
    Task<bool> VerifyUserPlaceAccess(int targetPlaceId, Staff? user = null);
    Task<bool> VerifyUserBusinessAccess(int businessId);
    Task EnsurePlaceExistsAsync(int placeId);
    Task EnsureBusinessExistsAsync(int businessId);
    Task EnsureTableExistsAsync(int tableId);
}
