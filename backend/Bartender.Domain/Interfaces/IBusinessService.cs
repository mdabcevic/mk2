using Bartender.Data.Enums;
using Bartender.Domain.DTO;
using Bartender.Domain.DTO.Business;

namespace Bartender.Domain.Interfaces;

public interface IBusinessService
{
    Task<ServiceResult<BusinessDto>> GetByIdAsync(int id);
    Task<ServiceResult<List<BusinessDto>>> GetAllAsync();
    Task<ServiceResult> AddAsync(UpsertBusinessDto dto);
    Task<ServiceResult> UpdateAsync(int id, UpsertBusinessDto dto);
    Task<ServiceResult> UpdateSubscriptionAsync(SubscriptionTier tier);
    Task<ServiceResult> DeleteAsync(int id);
}
