using BartenderBackend.Models;
using BartenderBackend.Repositories;

namespace BartenderBackend.Services;

public class BusinessService : IBusinessService
{
    private readonly IRepository<Business> _repository;

    public BusinessService(IRepository<Business> repository)
    {
        _repository = repository;
    }

    public async Task<Business?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Business>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task AddAsync(Business business)
    {
        if (business.OIB.Length != 11)
            throw new Exception("OIB must be 11 characters");

        await _repository.AddAsync(business);
    }

    public async Task UpdateAsync(Business business)
    {
        await _repository.UpdateAsync(business);
    }

    public async Task DeleteAsync(int id)
    {
        var business = await _repository.GetByIdAsync(id);
        if (business != null)
            await _repository.DeleteAsync(business);
    }
}
