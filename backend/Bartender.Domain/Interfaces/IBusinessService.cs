using Bartender.Data.Enums;
using Bartender.Domain.DTO.Business;

namespace Bartender.Domain.Interfaces;

public interface IBusinessService
{
    Task<BusinessDto> GetByIdAsync(int id);
    Task<List<BusinessDto>> GetAllAsync();
    Task AddAsync(UpsertBusinessDto dto);
    Task UpdateAsync(int id, UpsertBusinessDto dto);
    Task UpdateSubscriptionAsync(SubscriptionTier tier);
    Task DeleteAsync(int id);
}
