using Bartender.Data.Models;
using Bartender.Domain.DTO;

namespace Bartender.Domain.Interfaces;

public interface IValidationService
{
    Task<bool> VerifyUserGuestAccess(int orderTableId);
    Task<bool> VerifyUserPlaceAccess(int targetPlaceId, Staff? user = null);
    Task<bool> VerifyUserBusinessAccess(int businessId);
    Task<bool> VerifyProductAccess(int? businessId, bool isUpsertOperation, Staff? user = null);
    Task EnsurePlaceExistsAsync(int placeId);
    Task EnsureBusinessExistsAsync(int businessId);
    Task EnsureTableExistsAsync(int tableId);
}
