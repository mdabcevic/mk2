using Bartender.Data.Models;
using Bartender.Domain.Interfaces;

namespace Bartender.Domain.Services;

public class BusinessService(IRepository<Businesses> repository) : IBusinessService
{
    public async Task<Businesses?> GetByIdAsync(int id)
    {
        return await repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Businesses>> GetAllAsync()
    {
        return await repository.GetAllAsync();
    }

    public async Task AddAsync(Businesses business)
    {
        if (business.OIB.Length != 11)
            throw new KeyNotFoundException("OIB must be 11 characters");

        await repository.AddAsync(business);
    }

    public async Task UpdateAsync(Businesses business)
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
