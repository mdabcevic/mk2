using BartenderBackend.Interfaces;
using BartenderBackend.Models;

namespace BartenderBackend.Services;

public class BusinessService(IRepository<Business> repository) : IBusinessService
{
    public async Task<Business?> GetByIdAsync(int id)
    {
        return await repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Business>> GetAllAsync()
    {
        return await repository.GetAllAsync();
    }

    public async Task AddAsync(Business business)
    {
        if (business.OIB.Length != 11)
            throw new KeyNotFoundException("OIB must be 11 characters");

        await repository.AddAsync(business);
    }

    public async Task UpdateAsync(Business business)
    {
        await repository.UpdateAsync(business);
    }

    public async Task DeleteAsync(int id)
    {
        var business = await repository.GetByIdAsync(id);
        if (business != null)
            await repository.DeleteAsync(business);
    }
}
