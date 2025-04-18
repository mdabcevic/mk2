
using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface ITableRepository : IRepository<Tables>
{
    Task<List<Tables>> GetAllByPlaceAsync(int placeId);
    Task<bool> ExistsByLabelAsync(int placeId, string label);
    Task<Tables?> GetByPlaceLabelAsync(int placeId, string label);
    Task<Dictionary<string, Tables>> GetByPlaceAsLabelDictionaryAsync(int placeId);
    Task<List<Tables>> GetActiveByPlaceAsync(int placeId);
    Task<Tables?> GetBySaltAsync(string salt);
}
