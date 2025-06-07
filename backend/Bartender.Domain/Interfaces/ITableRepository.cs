using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface ITableRepository : IRepository<Table>
{
    Task<List<Table>> GetAllByPlaceAsync(int placeId);
    Task<bool> ExistsByLabelAsync(int placeId, string label);
    Task<Table?> GetByPlaceLabelAsync(int placeId, string label);
    Task<Dictionary<string, Table>> GetByPlaceAsLabelDictionaryAsync(int placeId);
    Task<List<Table>> GetActiveByPlaceAsync(int placeId);
    Task<Table?> GetBySaltAsync(string salt);
}
