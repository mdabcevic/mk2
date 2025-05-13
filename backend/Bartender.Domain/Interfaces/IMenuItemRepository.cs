
using Bartender.Data.Models;

namespace Bartender.Domain.Interfaces;

public interface IMenuItemRepository : IRepository<MenuItem>
{
    Task<List<MenuItem>> GetMenuItemsByPlaceIdAync(int placeId, bool onlyAvailable = false);
    Task<Dictionary<string, List<MenuItem>>> GetMenuItemsByPlaceIdGroupedAsync(int placeId, bool onlyAvailable = false);
    Task<Dictionary<Place, List<MenuItem>>> GetAllGroupedByPlaceAsync();
}
