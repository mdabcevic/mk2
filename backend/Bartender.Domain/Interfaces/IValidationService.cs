using Bartender.Data.Models;
using Bartender.Domain.DTO;

namespace Bartender.Domain.Interfaces;

public interface IValidationService
{
    Task<ServiceResult> VerifyUserGuestAccess(int orderTableId);
    Task<bool> VerifyUserPlaceAccess(int targetPlaceId, Staff? user = null);
    Task<bool> VerifyUserBusinessAccess(int businessId);
    Task<ServiceResult> EnsurePlaceExistsAsync(int placeId);
    Task<ServiceResult> EnsureBusinessExistsAsync(int businessId);
    Task<ServiceResult> EnsureTableExistsAsync(int tableId);
}
